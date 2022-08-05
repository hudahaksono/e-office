(function ($) {
    var crdial = "";
    /* WORKAROUND FOR IE */
    if (!Array.prototype.includes) {
        Object.defineProperty(Array.prototype, "includes", {
            enumerable: false,
            value: function (obj) {
                var newArr = this.filter(function (el) {
                    return el === obj;
                });
                return newArr.length > 0;
            }
        });
    }
    if (!Array.prototype.find) {
        Object.defineProperty(Array.prototype, 'find', {
            value: function (predicate) {
                if (this === null) {
                    throw new TypeError('"this" is null or not defined');
                }
                var o = Object(this);
                var len = o.length >>> 0;
                if (typeof predicate !== 'function') {
                    throw new TypeError('predicate must be a function');
                }
                var thisArg = arguments[1];
                var k = 0;
                while (k < len) {
                    var kValue = o[k];
                    if (predicate.call(thisArg, kValue, k, o)) {
                        return kValue;
                    }
                    k++;
                }
                return undefined;
            }
        });
    }
    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (str) {
            if (this.indexOf(str) === 0) {
                return true;
            } else {
                return false;
            }
        };
    }
    if (!String.prototype.endsWith) {
        String.prototype.endsWith = function (suffix) {
            return this.indexOf(suffix, this.length - suffix.length) !== -1;
        };
    }
    /* Get WFSUrl */
    function GetWFSUrl() {
        var rnd = Math.floor(Math.random() * WFSUrl.split(",").length);
        return WFSUrl.split(",")[rnd];
    }
    /* PREPARE ALL ON DOCUMENT READY */
    $(document).ready(function () {
        init();
        prepareLegend("#overlayitem", "#basemapitem", olm);
        prepareLiveSerch("#gSearch", "#clrSearch", olm);
    });
    /* CLEAR SELECTION */
    clearSelect = function () {
        var lyrs = olm.getLayers(), ilayer;
        lyrs.forEach(function (ly, idx) {
            if (ly.get("name") === "info") {
                ilayer = ly;
                return;
            }
        });
        ilayer.getSource().clear();
    };
    /* GOTO PERSIL */
    showBidangSipt = function (data) {
        var lyrs = olm.getLayers(), ilayer;
        lyrs.forEach(function (ly, idx) {
            if (ly.get("name") === "info") {
                ilayer = ly;
                return;
            }
        });
        if (ilayer) {
            var isource = ilayer.getSource();
            infoPopup.setPosition(undefined);
            isource.clear();
            var feats = isource.getFormat().readFeatures(data);
            if (feats && feats.length > 0) {
                var feat = feats[0];
                isource.addFeature(feat);
                //infoFeatures = [feat];
                olm.getView().fit(feat.getGeometry(), { duration: 1200, nearest: false });
                //infpgsz = 1;
                //var params = { featureId: feat.getId(), page: 1 },
                //    infocontent = document.getElementById("infocontent");
                //$(infocontent).load(DetilInfoAction, params);
                //infoPopup.setPosition(ol.extent.getCenter(feat.getGeometry().getExtent()));
            } else { alert("Tidak ada di peta."); }
        }
    };
    /* LIVE SEARCH */
    function prepareLiveSerch(inputid, clrbtnid, map, infoLayer) {
        /* Live Search */
        $(inputid).autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: GSearchUrl,
                    type: "POST",
                    data: { loc: encodeURIComponent(request.term) },
                    success: function (data) {
                        var results = [];
                        for (var d = 0; d < data.length; d++) {
                            results.push({ label: data[d].name, value: data[d].name, geom: data[d].geometry, fmtaddr: data[d].formatted_address });
                        }
                        response(results);
                    }
                });
            },
            minLength: 5,
            delay: 500,
            select: function (event, ui) {
                var sw = ui.item.geom.viewport.southwest,
                    ne = ui.item.geom.viewport.northeast,
                    vproj = map.getView().getProjection().getCode();
                var ex = ol.extent.boundingExtent([ol.proj.transform([sw.lng, sw.lat], 'EPSG:4326', vproj), ol.proj.transform([ne.lng, ne.lat], 'EPSG:4326', vproj)]);
                map.getView().fit(ex, { duration: 1200, nearest: true });
                if (!$(clrbtnid).hasClass("aktif")) $(clrbtnid).toggleClass("aktif");
            }
        }).data("ui-autocomplete")._renderItem = function (ul, item) {
            return $("<li>")
                .append($('<div>').addClass("ui-menu-item-wrapper").append($("<a>").addClass("title").append(item.label)).append($("<p>").append(item.fmtaddr)))
                .appendTo(ul);
        };
        $(clrbtnid).click(function (e) { $(inputid).val(""); $(this).toggleClass("aktif"); });
    }
    /* LEGEND ITEM */
    function prepareLegend(ovlid, bslid, map) {
        var currlyrdata, currlyritm;
        var lyrs = map.getLayers();
        lyrs.forEach(function (ly, idx) {
            var isBase = ly.get("baseLayer");
            /* Set Base Layer Legend */
            if (isBase) {
                createBaseLayerItem(ly, $(bslid));
            }
            else {
                /* Set Overlay Legend */
                createOverlayLayerItem(ly, $(ovlid), "layer");
            }
        });
        /* Sortable layer */
        $(".overlay-layers").sortable({
            update: function (ev, ui) {
                setLayerZIndex(this, ui);
            },
            handle: ".move"
        });
        /* Set base layer on radio change */
        $('[name="basemap"]').change(function () { chgBaseLyr(this.value); });
        $('[name="basemap"]:first').prop("checked", true).trigger("change");
        /* Set font size bigger for Edge and IE */
        if (navigator.userAgent.match(/edge/i) || navigator.userAgent.match(/trident/i)) {
            $(".ol-control").css("font-size", "12px");
        }
        /* Define dialog button action */
        $('#cfmdlg>input').click(function (evt) {
            evt.preventDefault();
            if ($(this).val() === "Batal") {
                // Cancel remove layer
                $.unblockUI(); return false;
            } else {
                if (crdial === "rmlayer") {
                    // Do remove layer
                    doRemoveLayer();
                }
            }
        });
        /* Listen layer's collection event */
        lyrs.on('remove', onLayersEvent.bind({ optype: "remove", baseitem: $(bslid), overlayitem: $(ovlid) }));
        lyrs.on('add', onLayersEvent.bind({ optype: "add", baseitem: $(bslid), overlayitem: $(ovlid) }));
        lyrs.on("change", function (evt) { console.log(evt); });
        /* Functions */
        function onLayersEvent(evt) {
            var ly = evt.element,
                isBase = ly.get("baseLayer"),
                item = isBase ? this.baseitem : this.overlayitem;
            if (isBase) {
                if (this.optype === "remove") {
                    $(item).find(':input[value="' + ly.get("name") + '"]').closest("li").remove();
                } else {
                    createBaseLayerItem(ly, item);
                    $('[name="basemap"]').change(function () { chgBaseLyr(this.value); });
                    ly.setZIndex(-1);
                }
            } else {
                if (this.optype === "remove") {
                    $(item).find('li[data-layer-type="layer"][data-layer-name="' + ly.get("name") + '"]').remove();
                } else {
                    createOverlayLayerItem(ly, item, "layer");
                }
            }
        }
        function createBaseLayerItem(ly, item) {
            var baseitem = $('<li role="menuitemradio" class="list-group-item"></li>'),
                baselabel = $('<label class="baselabel"></label>'),
                baseinput = $('<input type="radio" name="basemap" value="' + ly.get("name") + '" />'),
                basespan = $('<span class="checkmark"></span>');
            $(item).append(baseitem);
            baseitem.append(baselabel);
            baselabel.append(ly.get("title"));
            baselabel.append(baseinput);
            baselabel.append(basespan);
        }
        function createOverlayLayerItem(ly, item, type) {
            /* Handle tileWMS (later maybe handle other type) */
            var lySrc = ly.getSource();
            if (lySrc instanceof ol.source.TileWMS && lySrc.getParams()["LAYERS"] !== undefined) {
                /* Layer */
                var ovlitem, sublist;
                if (type === "layer") {
                    ovlitem = $('<li class="list-group-item" data-layer-type="layer" data-layer-name="' + ly.get("name") + '"></li>');
                    var lyhead = $('<label class="layer-header"></label>'),
                        lyctrl = $('<label class="layer-control"></label>'),
                        btnrm = $('<button type="button" class="btn-xs btn-danger remove-layer" title="Hapus layer"><span class="glyphicon glyphicon-trash"></span></button>');
                    lyhead.append(ly.get("title"));
                    lyhead.append('<input type="checkbox" data-toggle="collapse" data-target="#' + ly.get("name") + '" />');
                    lyhead.append('<span class="toggle-indicator move"></span>');
                    lyctrl.append('<input type="checkbox"' + (ly.getVisible() ? ' checked="checked"' : '') + ' aria-label="' + ly.get("title") + '" />');
                    lyctrl.append('<div class="sld round" title="Sembunyikan layer"></div>');
                    lyctrl.append(btnrm);
                    /* Confirm remove layer */
                    btnrm.click({ layer: ly, type: "layer", name: ly.get("name"), title: ly.get("title") }, confirmRemoveLayer);
                    ovlitem.append(lyhead);
                    ovlitem.append(lyctrl);
                    /* Trigger change layer visibility */
                    lyctrl.find("input").change({ layer: ly, type: "layer", name: ly.get("name") }, setLayerVisibility);
                    sublist = $('<ul id="' + ly.get("name") + '" class="overlay-sublayers list-group collapse" style="padding-top:5px"></ul>');
                    ovlitem.append(sublist);
                    $(item).find(".overlay-layers").append(ovlitem);
                } else {
                    ovlitem = $(item).find('li[data-layer-name="' + ly.get("name") + '"]');
                    sublist = ovlitem.find('#'.concat(ly.get("name")));
                    sublist.empty();
                }
                /* Sub layer */
                var sublyrs = lySrc.get("layers"),
                    vislyrs = lySrc.getParams()["LAYERS"].split(","),
                    wmsurls = lySrc.urls,
                    lgsrc = LayerProxy.concat(wmsurls[Math.floor(Math.random() * wmsurls.length)], "request=GetLegendGraphic&format=image/png&transparent=true&layer={0}"),
                    ordered = [], filters = [];
                for (var l = 0; l < sublyrs.length; l++) {
                    var lname = sublyrs[l]["name"],
                        sublyr = $('<li class="list-group-item" data-layer-type="sublayer" data-layer-name="' + lname + '"></li>'),
                        slhead = $('<label class="layer-header"></label>'),
                        slctrl = $('<label class="layer-control"></label>'),
                        //imglg = $('<div class="legend-ctr"><img id="' + lname.replace(":", "__") + '" class="collapse" alt="..." src="' + lgsrc.replace("{0}", lname) + '" /></div>'),
                        imglg = $('<div class="legend-ctr collapse" id="' + lname.replace(":", "__") + '"><img class="move" alt="..." src="' + lgsrc.replace("{0}", lname) + '" /></div>');
                    btnrm = $('<button type="button" class="btn-xs btn-danger remove-layer" title="Hapus layer"><span class="glyphicon glyphicon-trash"></span></button>');
                    slhead.append(sublyrs[l]["title"]);
                    slhead.append('<input type="checkbox" data-toggle="collapse" data-target="#' + lname.replace(":", "__") + '" />');
                    slhead.append('<span class="toggle-indicator move"></span>');
                    slctrl.append();
                    slctrl.append('<input type="checkbox"' + (vislyrs.includes(lname) ? ' checked="checked"' : '') + ' aria-label="' + sublyrs[l]["title"] + '" />');
                    slctrl.append('<div class="sld round" title="Sembunyikan layer"></div>');
                    slctrl.append(btnrm);
                    /* Trigger change sub-layer visibility */
                    slctrl.find("input").change({ layer: ly, type: "sublayer", name: lname }, setLayerVisibility);
                    sublyr.append(slhead);
                    sublyr.append(slctrl);
                    sublyr.append(imglg);
                    sublist.append(sublyr);
                    /* Confirm remove sub-layer */
                    btnrm.click({ layer: ly, type: "sublayer", name: lname, title: sublyrs[l]["title"] }, confirmRemoveLayer);
                    if (vislyrs.includes(lname)) {
                        ordered.push(lname);
                        filters.push(sublyrs[l]["filter"] ? sublyrs[l]["filter"] : "INCLUDE");
                    }
                }
                /* Update layer params */
                lySrc.updateParams({ LAYERS: ordered.reverse().join() });
                lySrc.updateParams({ CQL_FILTER: filters.reverse().join(";") });
                /* Sortable sublayers */
                sublist.sortable({
                    update: function (ev, ui) {
                        setLayerZIndex(this, ui);
                    },
                    handle: ".move"
                });
            }
        }
        function chgBaseLyr(val) {
            map.getLayers().forEach(function (lyr) { if (lyr.get("baseLayer")) lyr.setVisible(lyr.get("name") === val); });
        }
        function setLayerVisibility(evt) {
            var layer = evt.data.layer,
                src = layer.getSource(),
                srclayers = src.get("layers");
            if (evt.data.type === "layer") {
                if (src.getParams()["LAYERS"].trim()) {
                    layer.setVisible(this.checked);
                }
            } else {
                var layers = [], filters = [], clayer,
                    lis = $(this).closest("ul").find("li");
                for (var i = 0; i < lis.length; i++) {
                    if ($(lis[i]).find(".layer-control input")[0].checked) {
                        layers.push($(lis[i]).data("layer-name"));
                        clayer = srclayers.find(function (ly) { return ly["name"] === $(lis[i]).data("layer-name"); });
                        filters.push(clayer["filter"] ? clayer["filter"] : "INCLUDE");
                    }
                }
                src.updateParams({ LAYERS: layers.reverse().join() });
                src.updateParams({ CQL_FILTER: filters.reverse().join(";") });
                var input = $(this).closest("ul").parent("li").find(".layer-control input[aria-label='" + layer.get("title") + "']")[0];
                if (layers.length === 0) {
                    layer.setVisible(false);
                }
                else {
                    if (input && input.checked) {
                        layer.setVisible(true);
                    }
                }
            }
            /* Change tooltip */
            $(this).parent("label").find("div").attr("title", this.checked ? "Sembunyikan layer" : "Tampilkan layer");
        }
        function doRemoveLayer() {
            if (currlyrdata.type === "layer") {
                map.removeLayer(currlyrdata.layer);
            }
            else {
                var vislayers = currlyrdata.layer.getSource().getParams()["LAYERS"].split(",");
                var filters = [];
                vislayers.splice(vislayers.indexOf(currlyrdata.name), 1);
                if (currlyrdata.layer.getSource().get("layers")) {
                    var sublayers = currlyrdata.layer.getSource().get("layers"),
                        tlayer = sublayers.find(function (ly) { return ly["name"] === currlyrdata.name; });
                    sublayers.splice(sublayers.indexOf(tlayer), 1);
                }
                vislayers.forEach(function (lname) {
                    var clayer = currlyrdata.layer.getSource().get("layers").find(function (ly) { return ly["name"] === lname; });
                    filters.push(clayer["filter"] ? clayer["filter"] : "INCLUDE");
                });
                currlyrdata.layer.getSource().updateParams({ LAYERS: vislayers.reverse().join() });
                currlyrdata.layer.getSource().updateParams({ CQL_FILTER: filters.reverse().join(";") });
                $(currlyritm).closest("li").remove();
            }
            $.unblockUI();
        }
        function confirmRemoveLayer(evt) {
            $('#cfmdlg>p').html("Yakin menghapus layer <b>".concat(evt.data.title, "</b>?"));
            $.blockUI({ message: $('#cfmdlg') });
            crdial = "rmlayer";
            currlyrdata = evt.data;
            currlyritm = this;
        }
        function setLayerZIndex(obj, ui) {
            var ltype = ui.item.data("layer-type"),
                olyers = map.getLayers().getArray(),
                layers, layer;
            if (ltype === "layer") {
                //var layer,
                //    layers = $(obj).children("li");
                layers = $(obj).children("li");
                layers.each(function (idx, el) {
                    layer = olyers.find(function (ly) { return ly.get("name") === $(el).data("layer-name"); });
                    layer.setZIndex(layers.length - 1 - idx);
                });
            }
            else {
                //var layers = [], filters = [], clayer,
                //    layer = olyers.find(function (ly) { return ly.get("name") === obj.id; });
                layers = [];
                layer = olyers.find(function (ly) { return ly.get("name") === obj.id; });
                var filters = [], clayer;
                $(obj).children("li").each(function (idx, el) {
                    if ($(el).find(".layer-control input").prop("checked")) {
                        layers.push($(el).data("layer-name"));
                        clayer = layer.getSource().get("layers").find(function (ly) { return ly["name"] === $(el).data("layer-name"); });
                        filters.push(clayer["filter"] ? clayer["filter"] : "INCLUDE");
                    }
                });
                layer.getSource().updateParams({ LAYERS: layers.reverse().join() });
                layer.getSource().updateParams({ CQL_FILTER: filters.reverse().join(";") });
            }
        }
    }
    /* FEATURE INFO */
    function prepareFeatureInfo(map, ipopup, ilayer) {
        /* Info Popup */
        var infocontent = document.getElementById("infocontent"),
            infocloser = document.getElementById("infocloser"),
            isource = ilayer.getSource();
        infocloser.onclick = function () {
            ipopup.setPosition(undefined);
            this.blur();
            return false;
        };
        map.on('singleclick', function (evt) {
            //if (draw_source.getFeatures().length > 0) return;
            isource.clear();
            ipopup.setPosition(undefined);
            infoFeatures = [];
            infpgsz = 0;
            var viewResolution = (this.getView().getResolution()),
                px = this.getEventPixel(evt.originalEvent);
            ///* Feature Layer */
            //var feat = this.forEachFeatureAtPixel(px, function (feat, layer) { return feat; }, { layerFilter: function (lyr) { return lyr.get('name') === 'tslyr'; } });
            //if (feat) {
            //    var origfeats = feat.get('features');
            //    if (origfeats.length > 1) {
            //        var featsext = ol.extent.createEmpty();
            //        for (var f = 0; f < origfeats.length; f++) {
            //            ol.extent.extend(featsext, origfeats[f].getGeometry().getExtent());
            //        }
            //        this.getView().fit(featsext);
            //    } else {
            //        isource.clear();
            //        $(infocontent).html(buildFeatureInfo(origfeats[0]));
            //        isource.addFeature(origfeats[0]);
            //        ipopup.setPosition(evt.coordinate);
            //    }

            //    return;
            //}
            /* Tile layer */
            var hit = this.forEachLayerAtPixel(px, layatpix.bind({ evt: evt, viewResolution: viewResolution }), this, lyrFilter);
            if (hit) {
                ipopup.setPosition(evt.coordinate);
            }
        });
        function layatpix(lyr, colorval) {
            var evt = this.evt,
                viewResolution = this.viewResolution;
            if (lyr.get("name") === "atrbpn") {
                var infourl = lyr.getSource().getGetFeatureInfoUrl(evt.coordinate, viewResolution, olm.getView().getProjection().getCode(), { 'INFO_FORMAT': 'application/json' });
                if (infourl) {
                    $.blockUI({ message: '<div style=\"padding:10px\"><b>Sedang proses... </b><p>harap tunggu</p></div>' });
                    $.ajax({
                        url: InfoAction,
                        method: "POST",
                        data: {
                            infourl: infourl.concat("&FEATURE_COUNT=10")
                        },
                        success: function (response) {
                            var proj = new ol.format.GeoJSON().readProjection(response);
                            var feats;
                            if (olm.getView().getProjection().getCode().match(/\d+/)[0] === proj.getCode().match(/\d+/)[0]) { // HELP
                                feats = new ol.format.GeoJSON().readFeatures(response);
                            } else {
                                feats = new ol.format.GeoJSON({ featureProjection: olm.getView().getProjection() }).readFeatures(response);
                            }
                            infoFeatures = infoFeatures.concat(feats);
                            infpgsz += infoFeatures.length;
                            if (feats && feats.length > 0) {
                                isource.refresh();
                                var feat = infoFeatures[0];
                                detilInfo({ featureId: feat.getId(), page: 1 });
                            }
                            $.unblockUI();
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            $.unblockUI();
                        }
                    });
                    return true;
                }
            }
            return false;
        }
        function lyrFilter(lyrcandidate) {
            return lyrcandidate.get("name") === "atrbpn";
        }
        navDetilInfo = function (navType, currPage, pageSize) {
            var pg = (navType === "prev") ? currPage - 1 : currPage + 1;
            if (infoFeatures[pg - 1])
                detilInfo({ featureId: infoFeatures[pg - 1].getId(), page: pg });
        };
        function detilInfo(params) {
            $.blockUI({ message: '<div style=\"padding:10px\"><b>Sedang proses... </b><p>harap tunggu</p></div>' });
            isource.clear();
            $(infocontent).load(DetilInfoAction, params);
            var feat = infoFeatures.find(function (ft) { if (ft.getId() === params.featureId) return ft; });
            isource.addFeature(feat);
            $.unblockUI();
        }
    }
    /* MAP AND ALL RELATED STUFF */
    function init() {
        $("#peta").height($(window).outerHeight() - $(".nav_menu").outerHeight() - 3);
        $('#wrapper').accordion("refresh");
        /* Info Layer & style */
        var pointimage = new ol.style.Circle({
            radius: 5,
            fill: null,
            stroke: new ol.style.Stroke({ color: 'blue', width: 2 })
        });
        var styles = {
            'Point': new ol.style.Style({
                image: pointimage
            }),
            'LineString': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'green',
                    width: 1
                })
            }),
            'MultiLineString': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'green',
                    width: 1
                })
            }),
            'MultiPoint': new ol.style.Style({
                image: pointimage
            }),
            'MultiPolygon': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'blue',
                    width: 1
                }),
                fill: new ol.style.Fill({
                    color: 'rgba(255, 255, 0, 0.2)'
                })
            }),
            'Polygon': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'blue',
                    lineDash: [4],
                    width: 3
                }),
                fill: new ol.style.Fill({
                    color: 'rgba(0, 0, 255, 0.2)'
                })
            }),
            'GeometryCollection': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'magenta',
                    width: 2
                }),
                fill: new ol.style.Fill({
                    color: 'magenta'
                }),
                image: new ol.style.Circle({
                    radius: 10,
                    fill: null,
                    stroke: new ol.style.Stroke({
                        color: 'magenta'
                    })
                })
            }),
            'Circle': new ol.style.Style({
                stroke: new ol.style.Stroke({
                    color: 'red',
                    width: 2
                }),
                fill: new ol.style.Fill({
                    color: 'rgba(255,0,0,0.2)'
                })
            })
        };
        var styleFunction = function (feature) {
            return styles[feature.getGeometry().getType()];
        };
        /* Create an overlay to anchor the popup to the map. */
        var infopanel = document.getElementById("infopanel"),
            infopopup = new ol.Overlay({
                element: infopanel,
                autoPan: true,
                autoPanAnimation: { duration: 250 }
            }),
            infoSource = new ol.source.Vector({
                format: new ol.format.GeoJSON({
                    featureProjection: ol.proj.get('EPSG:3857')
                })
            }),
            infoLayer = new ol.layer.Vector({
                source: infoSource,
                style: styleFunction,
                name: "info"
            })//,
            //drwSource = new ol.source.Vector({
            //    format: new ol.format.GeoJSON({
            //        featureProjection: ol.proj.get('EPSG:3857')
            //    })
            //}),
            //drwLayer = new ol.layer.Vector({
            //    source: drwSource,
            //    name: "drawing"
            //}),
            //tdpSource = new ol.source.Vector({
            //    format: new ol.format.GeoJSON({
            //        featureProjection: ol.proj.get('EPSG:3857')
            //    })
            //}),
            //tdpLayer = new ol.layer.Vector({
            //    source: tdpSource,
            //    name: "terdampak",
            //    renderMode: "image"
            //});
        infoPopup = infopopup;
        /* Layer Persil */
        var plyr = new ol.layer.Tile({
            source: new ol.source.TileWMS({
                urls: WMSUrl.split(","),
                params: { LAYERS: "".concat(WMSStoreNS + ":PersilBerdasarkanJenisHak," + WMSStoreNS + ":PERSILSIPT"), TILED: true, CQL_FILTER: "INCLUDE;INCLUDE" },
                serverType: "geoserver",
                tileLoadFunction: loadMTile.bind({ lyrname: "atrbpn" })
            }),
            name: "atrbpn",
            title: "ATR/BPN Layer"
        });
        /* Add proprty 'layers' to source for further purpose */
        plyr.getSource().set("layers", [
            { name: "".concat(WMSStoreNS, ":PERSILSIPT"), title: "Bidang Pengadaan Tanah" },
            { name: "".concat(WMSStoreNS, ":PersilBerdasarkanJenisHak"), title: "Persil Berdasarkan Jenis Hak" },
            { name: "".concat(WMSStoreNS, ":ZONANILAITANAHNOLABEL"), title: "Zona Nilai Tanah", filter: "VALIDSAMPAI IS NULL" },
            { name: "".concat("tante:OBJEKTANTE"), title: "Tanah Terlantar", filter: "VALIDSAMPAI IS NULL" }
        ]);

        /* Create Custom Container Control */
        window.app = {};
        var app = window.app;
        app.ContainerControl = function (opt_options) {
            var options = opt_options || {};
            var element = document.createElement('div');
            element.className = 'ol-ctrlcontainer ol-unselectable ol-control';
            ol.control.Control.call(this, {
                element: element,
                target: options.target
            });
        };
        ol.inherits(app.ContainerControl, ol.control.Control);
        var myContainer = new app.ContainerControl();

        /* Create OpenLayers Map */
        olm = new ol.Map({
            controls: [myContainer],
            layers: [
                new ol.layer.Tile({
                    source: new ol.source.OSM(),
                    name: "osm",
                    title: "Open Street Map",
                    zIndex: 0,
                    baseLayer: true
                }),
                new ol.layer.Tile({
                    visible: false,
                    preload: Infinity,
                    baseLayer: true,
                    name: "bing",
                    title: "Bing Map Aerial",
                    zIndex: 0,
                    source: new ol.source.BingMaps({
                        key: BMK,
                        imagerySet: "AerialWithLabels"
                    })
                }),
                new ol.layer.Tile({
                    source: new ol.source.XYZ({
                        attributions: [new ol.Attribution({ html: "<a href='//www.atrbpn.go.id/' target='_blank'><span class='glyphicon glyphicon-copyright-mark'></span>ATR/BPN RI</a>" })],
                        url: pdurl,
                        tileLoadFunction: loadMTile.bind({ lyrname: "petadasar" })
                    }),
                    name: "petadasar",
                    title: "Peta Dasar Pertanahan",
                    zIndex: 0,
                    visible: false,
                    baseLayer: true
                }),
                plyr,
                infoLayer//,
                //drwLayer,
                //tdpLayer
            ],
            overlays: [infopopup],
            target: 'peta',
            view: new ol.View({
                center: [13100695.15185293, -264166.36975356936], //ol.proj.fromLonLat([4.8, 47.75]), || ol.proj.transform([117.68555, -2.372369], 'EPSG:4326', 'EPSG:3857')||ol.proj.fromLonLat([117.68555, -2.372369])
                zoom: 4
            })
        });
        /* Add Controls */
        olm.addControl(new ol.control.ScaleLine());
        olm.addControl(new ol.control.Attribution());
        var zoomctrl = new ol.control.Zoom({ target: myContainer.element });
        olm.addControl(zoomctrl);
        var zoomextctrl = new ol.control.ZoomToExtent({
            extent: [10719664, -1663463, 15450199, 1252149],
            tipLabel: "Zoom Indonesia",
            target: myContainer.element
        });
        olm.addControl(zoomextctrl);
        var rotatectrl = new ol.control.Rotate({ target: myContainer.element });
        olm.addControl(rotatectrl);
        $([zoomctrl.element, zoomextctrl.element, rotatectrl.element]).removeClass("ol-control ol-unselectable");

        /* Tile Load Function */
        function loadMTile(imageTile, src) {
            if (this.lyrname === "atrbpn") {
                imageTile.getImage().src = LayerProxy.concat(src);
            } else if (this.lyrname === "petadasar") {
                imageTile.getImage().src = src;
            }
        }
        $(".ol-zoom-extent button").html('<span class="fa fa-expand"></span>');
        /* Prepare Feature Info */
        prepareFeatureInfo(olm, infopopup, infoLayer);
        /* Add Select interaction */
        //var seli = new ol.interaction.Select({ multi: false, toggleCondition: ol.events.condition.never });
        //seli.set("name", "Seli");
        //olm.addInteraction(seli);
    }

    /* Load SHAPE file */
    //loadShp = function (e) {
    //    if (window.File && window.FileReader && window.FileList && window.Blob) {
    //        var input = $(this).get(0),
    //            numFiles = input.files ? input.files.length : 1;
    //        if (numFiles > 0) {
    //            if (shp) {
    //                var file = input.files[0],
    //                    reader = new FileReader();
    //                reader.onabort = function (evt) { $.unblockUI(); };
    //                reader.onerror = function (evt) { $.unblockUI(); };
    //                reader.onloadend = function (evt) { $.unblockUI(); };
    //                reader.onloadstart = function (evt) { $.blockUI({ message: '<div style=\"padding:10px\"><b>Sedang proses... </b><p>harap tunggu</p></div>' }); };
    //                reader.onload = function (evt) {
    //                    shp(evt.target.result).then(function (gjson) {
    //                        var proj = new ol.format.GeoJSON().readProjection(gjson);
    //                        var feats;
    //                        if (olm.getView().getProjection().getCode().match(/\d+/)[0] === proj.getCode().match(/\d+/)[0]) { // HELP
    //                            feats = new ol.format.GeoJSON().readFeatures(gjson);
    //                        } else {
    //                            feats = new ol.format.GeoJSON({ featureProjection: olm.getView().getProjection() }).readFeatures(gjson);
    //                        }
    //                        var lyrs = olm.getLayers(), ilayer;
    //                        lyrs.forEach(function (ly, idx) {
    //                            if (ly.get("name") === "drawing") {
    //                                ilayer = ly;
    //                                return;
    //                            }
    //                        });
    //                        if (ilayer) {
    //                            var isource = ilayer.getSource();
    //                            isource.clear();
    //                            isource.addFeatures(feats);
    //                            olm.getView().fit(isource.getExtent(), { duration: 1200, nearest: false });
    //                        }
    //                    }).catch(function (err) { showmsg("Error", err); });
    //                };
    //                reader.readAsArrayBuffer(file);
    //            }
    //        }
    //    } else {
    //        showmsg("Perhatian", "Browser anda tidak mendukung proses ini");
    //    }
    //};

    /* Cari bidang terdampak dari interaktif peta */
    //var formatAngka = function (nbr) {
    //    return nbr.toLocaleString('id-ID', { minimumIntegerDigits: 1, useGrouping: true });
    //},
    //    dtTerdampak = $('#tblhasilsml').DataTable({
    //        "ordering": false,
    //        "info": false,
    //        "bLengthChange": false,
    //        "bFilter": true,
    //        "buttons": [{
    //            "extend": 'excelHtml5',
    //            "sheetName": 'Simulasi',
    //            "filename": 'SimulasiPengadaanTanah',
    //            "title": function () { return $("#cbUseZNT") && $("#cbUseZNT").prop('checked') ? 'Simulasi Pengadaan Tanah menggunakan Zona Nilai Tanah(ZNT)' : 'Simulasi Pengadaan Tanah'; },
    //            "exportOptions": {
    //                "columns": ':visible',
    //                "format": {
    //                    "body": function (data, row, column, node) {
    //                        if (column >= 3 && column <= 5) {
    //                            return data.replace(/\./g, "").replace(/\,/g, ".");
    //                        }
    //                        return data;
    //                    }
    //                }
    //            }
    //        }],
    //        "columns": [
    //            {
    //                "data": null,
    //                "searchable": false,
    //                "orderable": false,
    //                "render": function (data, type, full, meta) {
    //                    if (type === 'display') {
    //                        return formatAngka(meta.row + 1);
    //                    }
    //                    return meta.row + 1;
    //                }
    //            },
    //            { "data": "NamaWilayah" },
    //            { "data": "Nib" },
    //            //{ "data": "TipeHak" },
    //            {
    //                "data": "LuasBidang",
    //                "className": "dt-body-right",
    //                "render": function (data, type, full, meta) {
    //                    if (type === 'display') {
    //                        return formatAngka(data);
    //                    }
    //                    return data;
    //                }
    //            },
    //            {
    //                "data": "LuasTerdampak",
    //                "className": "dt-body-right",
    //                "render": function (data, type, full, meta) {
    //                    if (type === 'display') {
    //                        return formatAngka(data);
    //                    }
    //                    return data;
    //                }
    //            },
    //            {
    //                "data": "NilaiZNT",
    //                "className": "dt-body-right",
    //                "render": function (data, type, full, meta) {
    //                    if (type === 'display') {
    //                        return formatAngka(data);
    //                    }
    //                    return data;
    //                }
    //            }
    //        ],
    //        "order": [[0, 'asc']],
    //        "language": {
    //            "lengthMenu": "Tampilkan _MENU_ recs. per hal.",
    //            "zeroRecords": "Data tidak ditemukan - maaf",
    //            "info": "Hal. _PAGE_ dari _PAGES_",
    //            "infoEmpty": "Tidak ada data tersedia",
    //            "infoFiltered": "(disaring dari total _MAX_ recs.)",
    //            "paginate": {
    //                "previous": "Prev"
    //            },
    //            "sSearch": "Cari"
    //        }
    //    });
    //dtTerdampak.on('draw.dt', function () {
    //    var PageInfo = dtTerdampak.page.info();
    //    dtTerdampak.column(0, { page: 'current' }).nodes().each(function (cell, i) {
    //        cell.innerHTML = i + 1 + PageInfo.start;
    //    });
    //});
    //var ismobile = /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i;
    //dtTerdampak.on("click", "tr", function () {
    //    if (!dtTerdampak.row(this).data()) { return; }
    //    if (!$(this).hasClass('selected')) {
    //        dtTerdampak.$('tr.selected').removeClass('selected');
    //        $(this).addClass('selected');
    //    }
    //    if (dtTerdampak.row(this).data().PersilId) {
    //        var pid = dtTerdampak.row(this).data().PersilId,
    //            tipe = "keliling";
    //        $.blockUI({ message: '<div style=\"padding:10px\"><b>Sedang proses... </b><p>harap tunggu</p></div>' });
    //        $.ajax({
    //            type: "POST",
    //            url: GetFeatureUrl,
    //            data: { pid: pid, tipe: tipe },
    //            success: function (data) {
    //                if (data.Status && data.Features.Data) {
    //                    try {
    //                        showBidangSipt(data.Features.Data);
    //                        if (ismobile.test(navigator.userAgent.toLowerCase())) {
    //                            $(".menu-toggle")[0].click();
    //                            $(".menu-toggle").trigger("click");
    //                        }
    //                    }
    //                    catch (err) {
    //                        showmsg("Error", err.message);
    //                    }
    //                } else {
    //                    showmsg("Perhatian", "Bidang belum dipetakan.");
    //                }
    //                $.unblockUI();
    //            },
    //            error: function (XMLHttpRequest, textStatus, errorThrown) {
    //                showmsg("Error", errorThrown);
    //                $.unblockUI();
    //            }
    //        });
    //    }
    //});
    //cariTerdampak = function (btn, val) {
    //    if (!val) {
    //        showmsg("Perhatian", "Pastikan nilai tanah diisi atau pilih Pakai Zona Nilai Tanah(ZNT)");
    //        return;
    //    }
    //    var sel = olm.getInteractions().getArray().find(function (i) { return i.get("name") === "Seli"; });
    //    if (sel && sel.getFeatures().getLength() > 0) {
    //        var wktgeom = new ol.format.WKT().writeFeatures([sel.getFeatures().item(0)], { dataProjection: ol.proj.get('EPSG:4326'), featureProjection: olm.getView().getProjection() });
    //        $(btn).attr("disabled", "disabled");
    //        $("#smresult").parent().block({ message: '<div style=\"padding:10px\"><b>Sedang proses... </b><p>harap tunggu</p></div>' });
    //        if (wktgeom.length > 0) {
    //            dtTerdampak.rows().remove().draw();
    //            dtTerdampak.search('');
    //            $.ajax({
    //                type: "POST",
    //                url: TerdampakUrl,
    //                data: { g: wktgeom, v: val },
    //                success: function (data, textStatus, XMLHttpRequest) {
    //                    if (data && data.Status) {
    //                        if (dtTerdampak) {
    //                            dtTerdampak.rows.add(data.Data).draw();
    //                        }
    //                    } else {
    //                        showmsg("Error", errorThrown);
    //                    }
    //                    $(btn).removeAttr("disabled");
    //                    $("#smresult").parent().unblock();
    //                },
    //                error: function (XMLHttpRequest, textStatus, errorThrown) {
    //                    showmsg("Error", errorThrown);
    //                    $(btn).removeAttr("disabled");
    //                    $("#smresult").parent().unblock();
    //                }
    //            });
    //        }
    //    } else {
    //        showmsg("Perhatian", "Tidak ada objek yang dipilih.");
    //    }
    //};
    //filterHasil = function (evt, obj) {
    //    if (dtTerdampak && dtTerdampak.data() && dtTerdampak.data().length > 0) {
    //        dtTerdampak.search(obj.value).draw();
    //    }
    //};
    /* HANDLES FOR RESIZE, MENU AND MASK */
    $(window).resize(function () {
        $("#peta").height($(window).outerHeight() - $(".nav_menu").outerHeight() - 3);
        if ($(".peta-side-menu").hasClass("active")) {
            $('#wrapper').accordion("refresh");
        }
    });
    $(".menu-toggle").on("click", function () {
        $("#pmask").toggleClass("active");
        $(".peta-side-menu").toggleClass("active");
        if ($(".peta-side-menu").hasClass("active")) {
            setTimeout(function () { $('#wrapper').accordion("refresh"); }, 100);
            $('#wrapper').accordion({ 'active': activeaccrd });
        } else {
            activeaccrd = $('#wrapper').accordion('option', 'active');
        }
    });
    $("#pmask").on("click", function () {
        $(".menu-toggle").click();
        $(".peta-side-menu").toggleClass("active");
        $(this).toggleClass("active");
    });
    $(".search-toggle").click(function (e) {
        e.preventDefault();
        var widg = $(".scwrapper");
        widg.toggleClass("collapsed expanded");
        if (widg.hasClass("collapsed")) {
            $("#gSearch").val("");
            if ($("#clrSearch").hasClass("aktif")) $("#clrSearch").toggleClass("aktif");
        }
    });
}(jQuery));