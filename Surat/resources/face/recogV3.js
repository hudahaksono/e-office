
const vid_presensi = document.getElementById("vid_presensi");
//const video = document.createElement("video");
//const canvasElement = document.getElementById("vid-canvas");
//const canvas2d = canvasElement.getContext("2d");
const videoModalContainer = document.getElementById("videoModal");
/*const presenceButton = document.getElementById("presence");*/
const pGeoLocation = "#pGeoLocation";
const label = "#pTest";
const hasil = "#pHasil";
const pResult = "#pResult";
const vidContainer = "#vid_container";
const pPosisi = "#pPosisi";
const hStatus = "#hStatus";

//let videoStream;
let interval;
let intLoc;
let scanning = false;
let expressionLists = [];

let latData = "";
let lonData = "";
let map;
let markersArray = [];

function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition, showError, { maximumAge: 600000, timeout: 5000, enableHighAccuracy: true });
        intLoc = setInterval(async () => {
            navigator.geolocation.getCurrentPosition(addmarker, showError, { maximumAge: 600000, timeout: 5000, enableHighAccuracy: true });
        }, 10000);
    }
    else {
        console.log("Geolocation is not supported by this browser.");
    }
}

function showPosition(position) {
    var lat = position.coords.latitude;
    var lon = position.coords.longitude;
    var acc = position.coords.accuracy;
    var latlon = new google.maps.LatLng(lat, lon)
    mapholder.style.height = '375px';
    mapholder.style.width = '100%';

    var myOptions = {
        center: latlon, zoom: 18,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        disableDefaultUI: true,
        //mapTypeControl: false,
        //streetViewControl: false,
        //fullscreenControl: false,
        navigationControlOptions: { style: google.maps.NavigationControlStyle.SMALL }
    };
    map = new google.maps.Map(mapholder, myOptions);
    addmarker(position)
    showOverlays()
    //set the zoom level to the circle's size
    //map.fitBounds(circle.getBounds());

}


function addmarker(position) {
    var lat = position.coords.latitude;
    var lon = position.coords.longitude;
    var acc = position.coords.accuracy;
    var latlon = new google.maps.LatLng(lat, lon)
    deleteOverlays()

    let marker = new google.maps.Marker({ position: latlon, map: map, title: "Posisi Anda!" });
    markersArray.push(marker)

    let circle = new google.maps.Circle({
        center: latlon,
        radius: acc,
        map: map,
        fillColor: "Blue",
        fillOpacity: 0.1,
        strokeColor: "Blue",
        strokeOpacity: 0.2
    });
    markersArray.push(circle)

    latData = lat;
    lonData = lon;
    $.ajax({
        type: "POST",
        url: cekJarak,
        data: { pLong: lonData, pLat: latData },
        success: function (data) {
            if (data.Status === false) {
                new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
            }
            else {
                var _acc = Math.round(acc) > 1000 ? Math.round(acc / 1000) + " km" : + Math.round(acc) + " m";
                $(pGeoLocation).html("Anda melakukan presensi dari lokasi ini : (akurasi " + _acc + ")");
                $(pPosisi).html(data.ReturnValue);
                $(hStatus).val(data.ReturnValue2);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
        }
    });
}

function showOverlays() {
    if (markersArray) {
        for (i in markersArray) {
            markersArray[i].setMap(map);
        }
    }
}

function deleteOverlays() {
    if (markersArray) {
        for (i in markersArray) {
            markersArray[i].setMap(null);
        }
        markersArray.length = 0;
    }
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

//presenceButton.onclick = () => {
//    try {
//        //videoModal.show();
//        var options = { "backdrop": "static", keyboard: true };
//        $('#videoModal').modal(options);
//        $('#videoModal').modal('show');
//        navigator.mediaDevices
//            //.getUserMedia({ video: { facingMode: "user" } })
//            .getUserMedia({ video: true, audio: false })
//            .then(stream => {
//                scanning = true;
//                expressionLists = [];
//                $(btnMulai).hide();
//                $(hasil).hide();
//                //videoStream = stream;
//                vid_presensi.srcObject = stream;
//                $(label).text("Please Wait...");
//                $(pResult).html("");
//                /*new PNotify({ title: 'Informasi', text: "Kamera Berhasil Diaktifkan", delay: 3000, styling: 'bootstrap3', addclass: 'dark' });*/
//            })
//            .catch(function (e) {
//                if (e.name === 'ConstraintNotSatisfiedError') {
//                    new PNotify({ title: 'Perhatian', text: "Perangkat tidak dapat membuka kamera", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
//                } else if (e.name === 'PermissionDeniedError') {
//                    new PNotify({ title: 'Perhatian', text: "Izin Kamera belum diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
//                } else if (e.name === 'NotFoundError') {
//                    new PNotify({ title: 'Perhatian', text: "Kamera tidak ditemukan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
//                } else if (e.name === 'NotAllowedError') {
//                    new PNotify({ title: 'Perhatian', text: "Izin Kamera tidak diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
//                }
//                closeVideo(true);
//                swal("Gagal Membuka Kamera", e, "warning");
//            });
//    } catch (e) {
//        closeVideo(true);
//        swal("Gagal Membuka Kamera", e, "warning");
//    }
//};

function getBase64Image(img) {
    var canvas1 = document.createElement("canvas");
    canvas1.width = img.width;
    canvas1.height = img.height;
    var ctx = canvas1.getContext("2d");
    ctx.drawImage(img, 0, 0, canvas1.width, canvas1.height);
    var dataURL = canvas1.toDataURL("image/png");
    return dataURL;
}

function getImg64() {
    navigator.clipboard.writeText(getBase64Image(document.getElementById("imgBase")));
}

//presenceButton.addEventListener("click", function () {
//    videoModal.show();
//});

//function tick() {
//    canvas2d.drawImage(vid_presensi, 0, 0, canvasElement.width, canvasElement.height);
//    scanning && requestAnimationFrame(tick);
//}

//var videoModal = new bootstrap.Modal(videoModalContainer, {
//    keyboard: false,
//});

//videoModalContainer.addEventListener(
//    "shown.bs.modal",
//    async function (event) {
//        scanning = true;
//        expressionLists = [];
//        $(btnMulai).hide();
//        $(hasil).hide();
//        var constraints = { audio: true, video: { width: 1280, height: 720 } };
//        navigator.mediaDevices.getUserMedia(constraints)
//            .then(function () {
//                new PNotify({ title: 'Informasi', text: "Kamera Berhasil Diaktifkan", delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
//            })
//            .catch(function (error) {
//                swal("Gagal Membuka Kamera", error, "warning");
//            });
//        videoStream = await navigator.mediaDevices.getUserMedia({
//            video: {},
//        });
//        vid_presensi.srcObject = videoStream;
//        $(label).text("Please Wait...");
//        $(pResult).html("");
//        //tick();
//    }
//);

//videoModalContainer.addEventListener("hidden.bs.modal", function (event) {
//    closeVideo(true);
//});

const addExpressions = (expressions) => {
    const keys = Object.keys(expressions);

    keys.forEach((key, index) => {
        const filteredItems = expressionLists.filter((item) => item == key);
        //console.log(filteredItems);

        if (filteredItems.length == 0) {
            if (expressions[key] >= 0.9) {
                expressionLists.push(key);
            }
        }

        //console.log(expressionLists);
    });
};

Promise.all([
    faceapi.nets.tinyFaceDetector.loadFromUri(modelsUrl + 'tiny_face_detector_model-weights_manifest.json'),
    faceapi.nets.faceLandmark68Net.loadFromUri(modelsUrl + 'face_landmark_68_model-weights_manifest.json'),
    faceapi.nets.faceRecognitionNet.loadFromUri(modelsUrl + 'face_recognition_model-weights_manifest.json'),
    /* faceapi.nets.ssdMobilenetv1.loadFromUri(modelsUrl + 'ssd_mobilenetv1_model-weights_manifest.json'),*/
    faceapi.nets.faceExpressionNet.loadFromUri(modelsUrl + 'face_expression_model-weights_manifest.json')
]).then(() => {
    console.log("Model Loaded");
    try {
        //videoModal.show();
        //var options = { "backdrop": "static", keyboard: true };
        //$('#videoModal').modal(options);
        //$('#videoModal').modal('show');
        navigator.mediaDevices
            .getUserMedia({ video: true, audio: false })
            .then(stream => {
                scanning = true;
                expressionLists = [];
                $(hasil).hide();
                vid_presensi.srcObject = stream;
                $(pResult).text("Silahkan Start Video");
            })
            .catch(function (e) {
                if (e.name === 'ConstraintNotSatisfiedError') {
                    new PNotify({ title: 'Perhatian', text: "Perangkat tidak dapat membuka kamera", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
                } else if (e.name === 'PermissionDeniedError') {
                    new PNotify({ title: 'Perhatian', text: "Izin Kamera belum diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
                } else if (e.name === 'NotFoundError') {
                    new PNotify({ title: 'Perhatian', text: "Kamera tidak ditemukan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
                } else if (e.name === 'NotAllowedError') {
                    new PNotify({ title: 'Perhatian', text: "Izin Kamera tidak diberikan", delay: 4000, styling: 'bootstrap3', addclass: 'dark' });
                }
                closeVideo(true);
                swal("Gagal Membuka Kamera", e, "warning");
            });
    } catch (e) {
        closeVideo(true);
        swal("Gagal Membuka Kamera", e, "warning");
    }

    vid_presensi.addEventListener("play", () => {
        try {
            clearInterval(intLoc);
            var txtConfirm = "";
            if (_par != "") {
                var _par = $(hStatus).val().split("|");
                var txtS = _par[0];
                var txtD = _par[1];
                var txtK = _par[2];
                var txtW = _par[3];
                var txtT = _par[4];
                //if (txtS == "WFO") {
                //    txtConfirm = "Simpan Presensi";
                //} else {
                //    txtConfirm = "Anda akan Presensi dengan jarak " + txtD + " dari Kantor, Pastikan Koordinat perangkat anda telah sesuai";
                //}
                var txtTanggal = txtT + " " + txtW;
                txtConfirm = "Anda akan Presensi " + txtS + " pada " + txtW + "\ndengan jarak " + txtD + " dari " + txtK;
                vid_presensi.srcObject.getTracks().forEach(function (track) {
                    track.enabled = true;
                });
                scanning = true;
                const canvas = faceapi.createCanvasFromMedia(vid_presensi);
                //modalBody.append(canvas);
                const displaySize = {
                    width: vid_presensi.width,
                    height: vid_presensi.height,
                };
                //console.log(displaySize);
                $(pResult).text("Harap Tunggu . . .");
                faceapi.matchDimensions(canvas, displaySize);
                interval = setInterval(async () => {
                    try {
                        const detections = await faceapi
                            .detectSingleFace(
                                vid_presensi,
                                new faceapi.TinyFaceDetectorOptions()
                            )
                            .withFaceLandmarks()
                            .withFaceExpressions()
                            .withFaceDescriptor();
                        if (detections) {
                            if (scanning)
                                $(pResult).text("Mengecek Wajah");
                            const base64toUrl = async (b64) => {
                                const res = await fetch(b64);
                                const blob = await res.blob();
                                const objectURL = URL.createObjectURL(blob);
                                return objectURL;
                            };

                            var imgB64 = await base64toUrl(getBase64Image(document.getElementById("imgBase")));

                            const image = await faceapi.fetchImage(
                                imgB64//imgPath//`{{ asset(auth()->user()->image_url) }}`
                            );
                            //console.log(imgPath);

                            const imageMatcher = await faceapi
                                .detectSingleFace(
                                    image,
                                    new faceapi.TinyFaceDetectorOptions()
                                )
                                .withFaceLandmarks()
                                .withFaceDescriptor();

                            const faceMatcher = new faceapi.FaceMatcher(
                                imageMatcher,
                                0.60
                            );

                            const result = faceMatcher.findBestMatch(
                                detections.descriptor
                            );

                            //console.log(result.label);
                            //if (result.label == "result.label") {
                            //    $(label).text("Wajah Dikenali");
                            //}
                            //console.log(expressionLists.length);
                            //console.log(result.label);
                            const resizedDetections = faceapi.resizeResults(
                                detections,
                                displaySize
                            );
                            if (result.label != "unknown") {
                                addExpressions(resizedDetections.expressions);
                                //console.log(expressionLists);
                                if (expressionLists.length < 2) {
                                    if (scanning)
                                        $(pResult).text("Ubah Ekspresi Anda [kurang " + (2 - expressionLists.length) + " ekspresi]");
                                    //$(hasil).show();
                                    //$(hasil).text("Mencocokan Ekspresi Wajah [" + expressionLists.length + " dari 2]");
                                }
                            } else {
                                if (scanning)
                                    $(pResult).text("Wajah Tidak Cocok");
                            }
                            canvas
                                .getContext("2d")
                                .clearRect(0, 0, canvas.width, canvas.height);
                            faceapi.draw.drawDetections(canvas, resizedDetections);
                            faceapi.draw.drawFaceLandmarks(canvas, resizedDetections);
                            faceapi.draw.drawFaceExpressions(canvas, resizedDetections);
                            // console.log(resizedDetections[0].expressions);

                            if (expressionLists.length >= 2) {
                                if (scanning) {
                                    scanning = false;
                                    closeVideo(true);

                                    $.ajax({
                                        type: "POST",
                                        url: cekPresensi,
                                        success: function (rst) {
                                            if (rst.Status === false) {
                                                new PNotify({ title: 'Perhatian', text: rst.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                                $(pResult).html("Gagal Melakukan Pengecekan Presensi,<br>Silahkan Buka Ulang Halaman ini.<br>" + data.Pesan);
                                            }
                                            else {
                                                if (rst.Pesan != "" || rst.Pesan != null) {
                                                    txtConfirm = rst.Pesan + "\n" + txtConfirm;
                                                }
                                                swal({
                                                    title: "",
                                                    text: txtConfirm,
                                                    type: "info",
                                                    showCancelButton: true,
                                                    closeOnConfirm: false,
                                                    confirmButtonColor: "#5AE02D",
                                                    confirmButtonText: "Simpan Presensi",
                                                    cancelButtonText: "Batal",
                                                    showLoaderOnConfirm: true
                                                },
                                                    function (doConfirm) {
                                                        if (doConfirm) {
                                                            $.ajax({
                                                                type: "POST",
                                                                url: simpanPresensi,
                                                                data: { pLong: lonData, pLat: latData, pWaktu: txtTanggal },
                                                                success: function (data) {
                                                                    if (data.Status === false) {
                                                                        new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                                                        $(pResult).html("Gagal Melakukan Presensi,<br>Silahkan Buka Ulang Halaman ini.<br>" + data.Pesan);
                                                                    }
                                                                    else {
                                                                        $(pResult).html(data.ReturnValue2);
                                                                    }
                                                                    closeloading();
                                                                },
                                                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                                                    swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                                                }
                                                            });
                                                        } else {
                                                            $(pResult).html("Presensi dibatalkan,<br>Silahkan Buka Ulang Halaman ini.");
                                                            closeloading();
                                                        }
                                                    });
                                            }
                                        },
                                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                                            swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                        }
                                    });

                                    //swal({
                                    //    title: "",
                                    //    text: txtConfirm,
                                    //    type: "info",
                                    //    showCancelButton: true,
                                    //    closeOnConfirm: false,
                                    //    confirmButtonColor: "#DD6B55",
                                    //    confirmButtonText: "Ya",
                                    //    cancelButtonText: "Batal",
                                    //    showLoaderOnConfirm: true
                                    //},
                                    //    function (isConfirm) {
                                    //        if (isConfirm) {
                                    //            $.ajax({
                                    //                type: "POST",
                                    //                url: cekPresensi,
                                    //                success: function (rst) {
                                    //                    if (rst.Status === false) {
                                    //                        new PNotify({ title: 'Perhatian', text: rst.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                    //                        $(pResult).html("Gagal Melakukan Presensi,<br>Silahkan Buka Ulang Halaman ini.<br>" + data.Pesan);
                                    //                    }
                                    //                    else {
                                    //                        if (rst.Pesan == "") {
                                    //                            $.ajax({
                                    //                                type: "POST",
                                    //                                url: simpanPresensi,
                                    //                                data: { pLong: lonData, pLat: latData },
                                    //                                success: function (data) {
                                    //                                    if (data.Status === false) {
                                    //                                        new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                    //                                        $(pResult).html("Gagal Melakukan Presensi,<br>Silahkan Buka Ulang Halaman ini.<br>" + data.Pesan);
                                    //                                    }
                                    //                                    else {
                                    //                                        if (latData != null && latData != "" && lonData != null && lonData != "") {

                                    //                                        }
                                    //                                        $(pResult).html(data.ReturnValue2);
                                    //                                    }
                                    //                                    closeloading();
                                    //                                },
                                    //                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    //                                    swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                    //                                }
                                    //                            });
                                    //                        } else {
                                    //                            swal({
                                    //                                title: "",
                                    //                                text: rst.Pesan,
                                    //                                type: "info",
                                    //                                showCancelButton: true,
                                    //                                closeOnConfirm: false,
                                    //                                confirmButtonColor: "#DD6B55",
                                    //                                confirmButtonText: "Ya",
                                    //                                cancelButtonText: "Batal",
                                    //                                showLoaderOnConfirm: true
                                    //                            },
                                    //                                function (doConfirm) {
                                    //                                    if (doConfirm) {
                                    //                                        $.ajax({
                                    //                                            type: "POST",
                                    //                                            url: simpanPresensi,
                                    //                                            data: { pLong: lonData, pLat: latData, pTime: txtW },
                                    //                                            success: function (data) {
                                    //                                                if (data.Status === false) {
                                    //                                                    new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                    //                                                    $(pResult).html("Gagal Melakukan Presensi,<br>Silahkan Buka Ulang Halaman ini.<br>" + data.Pesan);
                                    //                                                }
                                    //                                                else {
                                    //                                                    if (latData != null && latData != "" && lonData != null && lonData != "") {

                                    //                                                    }
                                    //                                                    $(pResult).html(data.ReturnValue2);
                                    //                                                }
                                    //                                                closeloading();
                                    //                                            },
                                    //                                            error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    //                                                swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                    //                                            }
                                    //                                        });
                                    //                                    } else {
                                    //                                        $(pResult).html("Presensi dibatalkan,<br>Silahkan Buka Ulang Halaman ini.");
                                    //                                        closeloading();
                                    //                                    }
                                    //                                });
                                    //                        }
                                    //                    }
                                    //                },
                                    //                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    //                    swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                    //                }
                                    //            });
                                    //        } else {
                                    //            $(pResult).html("Presensi dibatalkan,<br>Silahkan Buka Ulang Halaman ini.");
                                    //            closeloading();
                                    //        }
                                    //    });
                                }
                            }
                        }
                    } catch (e) {
                        //videoModal.hide();
                        closeVideo(true);
                        swal("Gagal Mengecek Wajah", e, "warning");
                    }
                }, 500);
            } else {
                closeVideo(false);
            }
        } catch (e) {
            //videoModal.hide();
            closeVideo(true);
            swal("Gagal Menjalankan Pengenal Wajah", e, "warning");
        }
    });
});

function isIOSDevice() {
    return !!navigator.platform && /iPad|iPhone|iPod/.test(navigator.platform);
}

function closeVideo(st) {
    scanning = false;
    if (st) {
        $(vidContainer).hide();
        if (vid_presensi.srcObject != null) {
            vid_presensi.srcObject.getTracks().forEach(function (track) {
                track.stop();
            });
        }
    } else {
        vid_presensi.srcObject.getTracks().forEach(function (track) {
            track.enabled = false;
        });
    }
    if (isIOSDevice())
        $("#vid_presensi")[0].webkitExitFullScreen();
   /* $(hasil).hide();*/
    clearInterval(interval);
    intLoc = setInterval(async () => {
        navigator.geolocation.getCurrentPosition(addmarker, showError);
    }, 10000);
   /* $('#videoModal').modal('hide');*/
}

//window.addEventListener("DOMContentLoaded", (event) => {

//});


function detectarPiscada(keypoints) {

    leftEye_l = 263
    leftEye_r = 362
    leftEye_t = 386
    leftEye_b = 374

    rightEye_l = 133
    rightEye_r = 33
    rightEye_t = 159
    rightEye_b = 145

    aL = euclidean_dist(keypoints[leftEye_t][0], keypoints[leftEye_t][1], keypoints[leftEye_b][0], keypoints[leftEye_b][1]);
    bL = euclidean_dist(keypoints[leftEye_l][0], keypoints[leftEye_l][1], keypoints[leftEye_r][0], keypoints[leftEye_r][1]);
    earLeft = aL / (2 * bL);

    aR = euclidean_dist(keypoints[rightEye_t][0], keypoints[rightEye_t][1], keypoints[rightEye_b][0], keypoints[rightEye_b][1]);
    bR = euclidean_dist(keypoints[rightEye_l][0], keypoints[rightEye_l][1], keypoints[rightEye_r][0], keypoints[rightEye_r][1]);
    earRight = aR / (2 * bR);

    console.log('-----> ' + earLeft + '\t' + earRight);

    if ((earLeft < 0.1) || (earRight < 0.1)) {
        return true;
    } else {
        return false;
    }

}

function euclidean_dist(x1, y1, x2, y2) {
    return Math.sqrt(Math.pow((x1 - x2), 2) + Math.pow((y1 - y2), 2));
};
