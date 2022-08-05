const history = {
    items: "",
    sort: function sortedUpItem() {
        let parr = this.items
        var items = parr.children('ul').children('li')
        $("#sortedHistory").html('')
        $("#sortedHistory").append(`<ul class="list-unstyled timeline"></ul>`)
        var fSorted = $("#sortedHistory ul")

        var count = 0
        for (var item of items) {
            var penerima = item.attributes["data-penerima"].nodeValue
            var pengirim = item.attributes["data-pengirim"].nodeValue
            var tier = item.attributes["data-tier"].nodeValue
            tier = parseInt(tier)
            //find frist
            if (!$(`#${pengirim}`).length) {
                var div = `<div id='${penerima}' style='padding-left:${tier}%'>
                    ${item.outerHTML}
                </div>`
                fSorted.append(div)
            } else {
                var div = `<div id='${penerima}'style='padding-left:${tier}%' class='hoverme'>${item.outerHTML}</div>`
                if (tier == 1) {
                    var div = `<div id='${penerima}' style='padding-left:${tier}%' class='hoverme'>
                    ${item.outerHTML}
                </div>`
                }
                $(`#${pengirim}`).append(div)
            }
        }
        this.items.hide()
        $("#sortedHistory").show()
    },
    hide: function () {
        this.items.show()
        $("#sortedHistory").hide()
    }
}
