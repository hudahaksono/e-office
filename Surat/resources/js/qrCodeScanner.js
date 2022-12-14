const qrreader = window.qrcode;

const video = document.createElement("video");
const canvasElement = document.getElementById("qr-canvas");
const canvas = canvasElement.getContext("2d");

const qrResult = document.getElementById("qr-result");
const qrManual = document.getElementById("qr-manual");
const btnScanQR = document.getElementById("btn-scan-qr");
const mapholder = document.getElementById("mapholder");
const btnClose = "#btnClose";
const txtKode = "#txtKode";
const pGeoLocation = "#pGeoLocation";
const latData = "#latData";
const lonData = "#lonData";

let scanning = false;


qrreader.callback = res => {
    if (res) {
        $(txtKode).val(res);
        scanning = false;
        if (video.srcObject != null) {
            video.srcObject.getTracks().forEach(track => {
                track.stop();
            });
        }
        canvasElement.hidden = true;
        btnScanQR.hidden = false;
        qrManual.hidden = false;
        $(btnClose).hide();
        if (tipe == "presensi") {
            doPresensi();
        } else if (tipe == "daftar") {
            doTambahPeserta();
        } else if (tipe == "validasi") {
            doValidasi();
        }
    }
};

function closeReader() {
    $(txtKode).val("");
    scanning = false;
    if (video.srcObject != null) {
        video.srcObject.getTracks().forEach(track => {
            track.stop();
        });
    }
    canvasElement.hidden = true;
    btnScanQR.hidden = false;
    qrManual.hidden = false;
    $(btnClose).hide();
    clearTimeout(mytimeout);
}

btnScanQR.onclick = () => {
    qrManual.hidden = true;
    $(btnClose).show();
    navigator.mediaDevices
        .getUserMedia({ video: { facingMode: "environment" } })
        .then(function (stream) {
            scanning = true;
            /*qrResult.hidden = true;*/
            btnScanQR.hidden = true;
            canvasElement.hidden = false;
            video.setAttribute("playsinline", true); // required to tell iOS safari we don't want fullscreen
            video.srcObject = stream;
            video.play();
            tick();
            scan();
        })
        .catch(function (e) {
            if (e.name === 'ConstraintNotSatisfiedError') {
                new PNotify({ title: 'Perhatian', text: "Perangkat tidak dapat membuka kamera", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            } else if (e.name === 'PermissionDeniedError') {
                new PNotify({ title: 'Perhatian', text: "Izin Kamera belum diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            } else if (e.name === 'NotFoundError') {
                new PNotify({ title: 'Perhatian', text: "Kamera tidak ditemukan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            }
            console.log(e);
            closeReader();
        });
};

function tick() {
    canvasElement.height = video.videoHeight;
    canvasElement.width = video.videoWidth;
    canvas.drawImage(video, 0, 0, canvasElement.width, canvasElement.height);

    scanning && requestAnimationFrame(tick);
}

function scan() {
    try {
        qrreader.decode();
    } catch (e) {
        mytimeout = setTimeout(scan, 300);
        /*console.log(e);*/
    }
}

function doPresensi() {
    showloading("Presensi dalam proses");
    $.ajax({
        type: "POST",
        url: CekAbsenUrl,
        data: { mCd: encodeURI($(txtKode).val().trim()), plong: $(lonData).html(), plat: $(latData).html() },
        success: function (data) {
            if (data.Status === false) {
                new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                $(txtKode).val("");
                closeloading();
            }
            else {
                new PNotify({ title: 'Informasi', text: data.Pesan, delay: 8000, styling: 'bootstrap3', addclass: 'dark' });
                swal("Informasi", data.Pesan, "success")
                $.ajax({
                    type: "POST",
                    url: CekRapatUrl,
                    data: { mCd: $(txtKode).val(), sQc: false },
                    success: function (rpt) {
                        $("#popuptitle").html(rpt.Judul + " Berhasil");
                        $("#txtNamaKegiatan").val(rpt.Judul);
                        $("#txtTanggalKegiatan").val(rpt.Tanggal);
                        $("#txtKeteranganKegiatan").val(rpt.Keterangan);
                        $("#txtNamaPeserta").val(rpt.NamaPeserta);
                        rapatonlineid = rpt.RapatOnlineId;
                        $("#container").hide();
                        $("#divResult").show();
                        //dtableDaftarPresensi.ajax.reload(null, true);\
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                    }
                });
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
        }
    });
}

function doTambahPeserta() {
    showloading("Pendaftaran dalam proses");
    $.ajax({
        type: "POST",
        url: TambahUrl,
        data: { mCd: $(txtKode).val() },
        success: function (data) {
            if (data.Status === false) {
                new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                $(txtKode).val("");
            }
            else {
                new PNotify({ title: 'Informasi', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                $("#container").hide();
                $("#divResult").show();

                $.ajax({
                    type: "POST",
                    url: CekRapatUrl,
                    data: { mCd: $(txtKode).val() },
                    success: function (rpt) {
                        $("#popuptitle").html("Pendaftaran Peserta");
                        $("#txtNamaKegiatan").val(rpt.Judul);
                        $("#txtTanggalKegiatan").val(rpt.Tanggal);
                        $("#txtKeteranganKegiatan").val(rpt.Keterangan);
                        $("#txtNamaPeserta").val(rpt.NamaPeserta);
                        $("#imgqrcode").attr("src", rpt.QRCode);
                        rapatonlineid = rpt.RapatOnlineId;
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                    }
                });
            }
            closeloading();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
        }
    });
}

function getPosition() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(addPosition, showError);
    }
    else {
        console.log("Geolocation is not supported by this browser.");
    }
}

function addPosition(position) {
    var lat = position.coords.latitude;
    var lon = position.coords.longitude;
    $(latData).html(lat);
    $(lonData).html(lon);
}

function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition, showError);
    }
    else {
        console.log("Geolocation is not supported by this browser.");
    }
}

function showPosition(position) {
    var lat = position.coords.latitude;
    var lon = position.coords.longitude;
    var latlon = new google.maps.LatLng(lat, lon)    
    mapholder.style.height = '250px';
    mapholder.style.width = '100%';

    var myOptions = {
        center: latlon, zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        mapTypeControl: false,
        navigationControlOptions: { style: google.maps.NavigationControlStyle.SMALL }
    };
    var map = new google.maps.Map(mapholder, myOptions);
    var marker = new google.maps.Marker({ position: latlon, map: map, title: "Posisi Anda!" });
    $(latData).html(lat);
    $(lonData).html(lon);
}

function showError(error) {
    switch (error.code) {
        case error.PERMISSION_DENIED:
            new PNotify({ title: 'Perhatian', text: "Izin 'Geolocation' belum diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            break;
        case error.POSITION_UNAVAILABLE:
            new PNotify({ title: 'Perhatian', text: "Informasi 'Geolocation' tidak tersedia", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            break;
        case error.TIMEOUT:
            new PNotify({ title: 'Perhatian', text: "Gagal mendapatkan lokasi perangkat", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            break;
        case error.UNKNOWN_ERROR:
            new PNotify({ title: 'Perhatian', text: "Gagal menemukan lokasi perangkat", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
            break;
    }
    $(pGeoLocation).text("Presensi anda akan terkirim tanpa lokasi perangkat");
    console.log(error);
}

function doValidasi() {
    qrManual.hidden = true;
    showloading("Validasi dalam proses");
    $.ajax({
        type: "POST",
        url: CekPesertaUrl,
        data: { pCd: $(txtKode).val(), plong: $(lonData).html(), plat: $(latData).html() },
        success: function (data) {
            if (data.Status === false) {
                new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 8000, styling: 'bootstrap3', addclass: 'dark' });
            }
            else {
                new PNotify({ title: 'Informasi', text: data.Pesan, delay: 5000, styling: 'bootstrap3', addclass: 'dark' });
            }
            $(txtKode).val("");
            closeloading();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
        }
    });
}
