window.addEventListener("DOMContentLoaded", (event) => {
    const vid_presensi = document.getElementById("vid_presensi");
    //const video = document.createElement("video");
    //const canvasElement = document.getElementById("vid-canvas");
    //const canvas2d = canvasElement.getContext("2d");
    const presenceButton = document.getElementById("presence");
    const modalBody = document.getElementById("face-container");
    const label = "#pTest";
    const hasil = "#pHasil";
    const btnMulai = "#presence";

    let videoStream;
    let interval;
    //let scanning = false;

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

    presenceButton.onclick = () => {
        try {
            navigator.mediaDevices
                .getUserMedia({ video: { facingMode: "user" } })
                .then(function (stream) {
                    scanning = true;
                    modalBody.hidden = false;
                    expressionLists = [];
                    $(btnMulai).hide();
                    $(hasil).hide();
                    videoStream = stream;
                    vid_presensi.srcObject = videoStream;
                    $(label).text("Please Wait...");
                    //tick();
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

    //function tick() {
    //    canvas2d.drawImage(vid_presensi, 0, 0, canvasElement.width, canvasElement.height);
    //    scanning && requestAnimationFrame(tick);
    //}

    //var videoModal = new bootstrap.Modal(videoModalContainer, {
    //    keyboard: false,
    //});

    //presenceButton.addEventListener("click", function () {
    //    videoModal.show();
    //});

    //videoModalContainer.addEventListener(
    //    "shown.bs.modal",
    //    async function (event) {
    //        // console.log(videoStream);
    //        videoStream = await navigator.mediaDevices.getUserMedia({
    //            video: {},
    //        });
    //        video.srcObject = videoStream;
    //    }
    //);

    //videoModalContainer.addEventListener("hidden.bs.modal", function (event) {
    //    videoStream.getTracks().forEach(function (track) {
    //        track.stop();
    //    });
    //    clearInterval(interval);
    //});

    let expressionLists = [];

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
                            0.55
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
                                $(label).html("Selamat Datang, " + _namaPegawai + "<br /> [" + new Date().toLocaleString() + "]");
                            }
                            closeVideo();
                            //new PNotify({ title: 'Informasi', text: "Wajah Dikenali", delay: 3000, styling: 'bootstrap3', addclass: 'dark' });
                        }
                    }
                } catch (e) {
                    closeVideo();
                    swal("Gagal Mengecek Wajah", e, "warning");
                }
            }, 500);
        });
    });

    function closeVideo() {
        scanning = false;
        $(btnMulai).show();
        videoStream.getTracks().forEach(function (track) {
            track.stop();
        });
        $(hasil).hide();
        clearInterval(interval);
        modalBody.hidden = true;
    }
});
