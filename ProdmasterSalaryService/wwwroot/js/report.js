function RefreshReportsTable() {
    data = GetDataForRefreshTable();
    $.ajax({
        url: "/report/refreshReportsTable",
        method: 'GET',
        data,
        success: (html) => {
            $('#report').html(html);
            $('#report').removeClass("loading");
        },
        error: (response) => {
            console.log(response);
        }
    });
}

function GetDataForRefreshTable() {
    let year = $("#selectYear").val();
    let month = $("#selectMonth").val();
    data = {
        year: year,
        month: month,
    }
    return data;
}

const modal = $('#dialogContent');
modal.addEventListener('shown.bs.modal', () => {
    modal.focus()
})

function GetMenu(element) {
    if ((!$(element).hasClass("passed-day") && !$(element).hasClass("remote-day") && !$(element).hasClass("holiday-day"))) { return; }
    let day = $(element).text();
    dateString = GetDateString(day);
    data = {
        dateString: dateString,
    };
    $.ajax({
        url: "/report/getMenuForDay",
        method: 'GET',
        data,
        success: (html) => {
            $('#dialogContent').html(html);
            modal.modal('show');
        },
        error: (response) => {
            console.log(response);
        }
    });
}

function CloseModal() { 
    modal.modal('hide');
}

function MarkAsRemote() {
    let dateString = $("#selectedDay").text();
    data = {
        dateString: dateString,
    };
    modal.modal('hide');
    $.ajax({
        url: "/report/setDateRemote",
        method: 'GET',
        data,
        success: (html) => {
            $('#dialogContent').html(html);
            RefreshReportsTable();
        },
        error: (response) => {
            console.log(response);
        }
    });
    
}

function MarkAsSkip() {
    let dateString = $("#selectedDay").text();
    data = {
        dateString: dateString,
    };
    modal.modal('hide');
    $.ajax({
        url: "/report/setDateSkip",
        method: 'GET',
        data,
        success: (html) => {
            $('#dialogContent').html(html);
            RefreshReportsTable();
        },
        error: (response) => {
            console.log(response);
        }
    });

}

function MarkAsHoliday() {
    let dateString = $("#selectedDay").text();
    data = {
        dateString: dateString,
    };
    modal.modal('hide');
    $.ajax({
        url: "/report/setHoliday",
        method: 'GET',
        data,
        success: (html) => {
            $('#dialogContent').html(html);
            RefreshReportsTable();
        },
        error: (response) => {
            console.log(response);
        }
    });

}

function GetDateString(day) {
    
    let year = $("#selectYear").val();
    let month = $("#selectMonth").val() - 1;
    let date = new Date(year, month, day, 0, 0, 0, 0);
    return date.toISOString();
}