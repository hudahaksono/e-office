
const vid_presensi = document.getElementById("vid_presensi");
//const video = document.createElement("video");
//const canvasElement = document.getElementById("vid-canvas");
//const canvas2d = canvasElement.getContext("2d");
const videoModalContainer = document.getElementById("videoModal");
const presenceButton = document.getElementById("presence");
const pGeoLocation = "#pGeoLocation";
const label = "#pTest";
const hasil = "#pHasil";
const pResult = "#pResult";
const btnMulai = "#presence";

//let videoStream;
let interval;
let scanning = false;
let expressionLists = [];

let latData = "";
let lonData = "";

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
    mapholder.style.height = '425px';
    mapholder.style.width = '100%';

    var myOptions = {
        center: latlon, zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        mapTypeControl: false,
        navigationControlOptions: { style: google.maps.NavigationControlStyle.SMALL }
    };
    var map = new google.maps.Map(mapholder, myOptions);
    var marker = new google.maps.Marker({ position: latlon, map: map, title: "Posisi Anda!" });
    latData = lat;
    lonData = lon;
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

presenceButton.onclick = () => {
    try {
        //videoModal.show();
        var options = { "backdrop": "static", keyboard: true };
        $('#videoModal').modal(options);
        $('#videoModal').modal('show');
        navigator.mediaDevices
            //.getUserMedia({ video: { facingMode: "user" } })
            .getUserMedia({ video: true, audio: false })
            .then(stream => {
                scanning = true;
                expressionLists = [];
                $(btnMulai).hide();
                $(hasil).hide();
                //videoStream = stream;
                vid_presensi.srcObject = stream;
                $(label).text("Please Wait...");
                $(pResult).html("");
                /*new PNotify({ title: 'Informasi', text: "Kamera Berhasil Diaktifkan", delay: 3000, styling: 'bootstrap3', addclass: 'dark' });*/
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
                closeVideo();
                swal("Gagal Membuka Kamera", e, "warning");
            });
    } catch (e) {
        closeVideo();
        swal("Gagal Membuka Kamera", e, "warning");
    }
};

//function getBase64Image(img) {
//    var canvas1 = document.createElement("canvas");
//    canvas1.width = img.width;
//    canvas1.height = img.height;
//    var ctx = canvas1.getContext("2d");
//    ctx.drawImage(img, 0, 0, canvas1.width, canvas1.height);
//    var dataURL = canvas1.toDataURL("image/png");
//    return dataURL;
//}

//function getImg64() {
//    navigator.clipboard.writeText(getBase64Image(document.getElementById("imgBase")));
//}

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
//    closeVideo();
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
    $("#presence").show();
    vid_presensi.addEventListener("play", () => {
        try {
            const canvas = faceapi.createCanvasFromMedia(vid_presensi);
            //modalBody.append(canvas);
            const displaySize = {
                width: vid_presensi.width,
                height: vid_presensi.height,
            };
            //console.log(displaySize);
            $(label).text("Start Engine");
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
                            $(label).text("Mengecek Wajah");
                        //const base64toUrl = async (b64) => {
                        //    const res = await fetch(b64);
                        //    const blob = await res.blob();
                        //    const objectURL = URL.createObjectURL(blob);
                        //    return objectURL;
                        //};

                        //var imgB64 = await base64toUrl(getBase64Image(document.getElementById("imgBase")));

                        const image = await faceapi.fetchImage(
                            imgPath//imgB64//`{{ asset(auth()->user()->image_url) }}`
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
                            0.65
                        );

                        const result = faceMatcher.findBestMatch(
                            detections.descriptor
                        );

                        //console.log(result);

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
                                    $(label).text("Wajah Cocok");
                                $(hasil).show();
                                $(hasil).text("Mencocokan Ekspresi Wajah [" + expressionLists.length + " dari 2]");
                            }
                        } else {
                            if (scanning)
                                $(label).text("Wajah Tidak Cocok");
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
                                getLocation();
                                $.ajax({
                                    type: "POST",
                                    url: simpanPresensi,
                                    data: { pLong: lonData, pLat: latData },
                                    success: function (data) {
                                        if (data.Status === false) {
                                            new PNotify({ title: 'Perhatian', text: data.Pesan, delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                                        }
                                        else {
                                            if (latData != null && latData != "" && lonData != null && lonData != "") {

                                            }
                                            $(pResult).html(data.ReturnValue2);
                                            //$(pResult).html("Selamat Datang, " + _namaPegawai + "<br /> [" + new Date().toLocaleString() + "]");
                                            //videoModal.hide();
                                            closeVideo();
                                        }
                                    },
                                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                                        swal(textStatus, "Terjadi Kesalahan \n" + errorThrown, "warning");
                                    }
                                });
                            }
                        }
                    }
                } catch (e) {
                    //videoModal.hide();
                    closeVideo();
                    swal("Gagal Mengecek Wajah", e, "warning");
                }
            }, 500);
        } catch (e) {
            //videoModal.hide();
            closeVideo();
            swal("Gagal Menjalankan Pengenal Wajah", e, "warning");
        }
    });
});

function closeVideo() {
    scanning = false;
    $(btnMulai).show();
    if (vid_presensi.srcObject != null) {
        vid_presensi.srcObject.getTracks().forEach(function (track) {
            track.stop();
        });
    }
    $(hasil).hide();
    $(label).html("");
    clearInterval(interval);
    $('#videoModal').modal('hide');
}

//window.addEventListener("DOMContentLoaded", (event) => {

//});
