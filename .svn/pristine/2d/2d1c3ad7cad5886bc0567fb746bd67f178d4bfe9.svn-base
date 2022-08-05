var page = 0,
    formToPost = null,
    inCallback = false,
    hasReachedEndOfInfiniteScroll = false;

var scrollHandler = function () {
    //$('#loading').html(($(window).scrollTop()).toString() + '-' + getDocHeight().toString() + '-' + $(window).height().toString());
    if (hasReachedEndOfInfiniteScroll == false &&
            (Math.round($(window).scrollTop())+$(window).height() > $(document).height() - 100)) {
        
        loadMoreToInfiniteScrollTable(moreRowsUrl);
        
    }
}

var divScrollHandler = function () {
    
    if (hasReachedEndOfInfiniteScroll == false &&
            (Math.round($(window).scrollTop()) + $(window).height() > $(document).height() - 100)) {
        loadMoreToInfiniteScrollTable(moreRowsUrl);
    }
}

var divScrollHandlerWithForm = function () {
    if (hasReachedEndOfInfiniteScroll == false &&
            (Math.round(divToListen.scrollTop() + divToListen.innerHeight()) >= divToListen[0].scrollHeight)) {
        loadMoreWithFormToInfiniteScrollTable(moreRowsUrl);
    }
}

var ulScrollHandler = function () {
    if (hasReachedEndOfInfiniteScroll == false &&
            (Math.round($(window).scrollTop()) == $(document).height() - $(window).height())) {
        loadMoreToInfiniteScrollUl(moreRowsUrl);
    }
}

function loadMoreToInfiniteScrollUl(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "pageNum=" + page,
            success: function (data, textstatus) {
                if (data != '') {
                    $("ul.infinite-scroll").append(data);
                }
                else {
                    page = -1;
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function loadMoreWithFormToInfiniteScrollTable(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        if (formToPost != null) { myData = formToPost.serialize() + "&pageNum=" + page } else { myData = "pageNum=" + page }
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'POST',
            url: loadMoreRowsUrl,
            //data: theform.serialize() + "&pageNum=" + page,
            data : myData,
            success: function (res, textstatus) {
                if (res != 'noresults') {
                    $("table.infinite-scroll > tbody").append(res);
                    $("table.infinite-scroll > tbody > tr:even").addClass("alt-row-class");
                    $("table.infinite-scroll > tbody > tr:odd").removeClass("alt-row-class");
                }
                else {
                    showNoMoreRecords();
                    page = -1;
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function loadMoreToInfiniteScrollTable(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        if (formToPost != null) { myData = formToPost.serialize() + "&pageNum=" + page } else { myData = "pageNum=" + page };
        $("div#loading").show();
        $.ajax({
            type: 'POST',
            url: loadMoreRowsUrl,
            //data: $('#frmFindBerkas').serialize() + "&pageNum=" + page,
            data: myData,
            success: function (data, textstatus) {
                if (data != 'noresults') {
                    $("table.infinite-scroll > tbody").append(data);
                    $("table.infinite-scroll > tbody > tr:even").addClass("alt-row-class");
                    $("table.infinite-scroll > tbody > tr:odd").removeClass("alt-row-class");
                }
                else {
                    showNoMoreRecords();
                    page = -1;
                }

                inCallback = false;
                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function showNoMoreRecords() {
    hasReachedEndOfInfiniteScroll = true;
}

function resetInfiniteScroll() {
    page = 0,
    inCallback = false,
    hasReachedEndOfInfiniteScroll = false,
    formToPost = null;
}