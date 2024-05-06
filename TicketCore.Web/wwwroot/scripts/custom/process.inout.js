function processin() {
    $.confirm({
        title: 'Xác nhận!',
        type: 'green',
        content: 'Do you want to Check-In for day?',
        buttons: {
            confirm: function () {
                $("#loader").show();

                $.ajax({
                    url: "/User/CheckIn/InProcess",
                    type: "POST",

                    data: { values: 'Process' },
                    success: function (data) {
                        if (data == "OK") {

                            $.alert({
                                title: 'Message!',
                                content: 'You have checkedIn for the day Successfully',
                                type: 'green'
                            });

                            location.href = "/User/MyDashboard/Dashboard";
                        }
                        else {

                            $.alert({
                                title: 'Message!',
                                content: 'Error While CheckIn Please Logout and Log in Again',
                                type: 'red'
                            });

                          
                        }
                    }
                });
                $("#loader").hide();
            },
            cancel: function () {
                $.alert('Đã hủy!');
                $("#loader").hide();
            }
        }
    });





}


function processout() {

    $.confirm({
        title: 'Xác nhận!',
        type: 'red',
        content: 'Do you want to Check-Out ?',
        buttons: {
            confirm: function () {
                $.ajax({
                    url: "/User/CheckIn/OutProcess",
                    type: "POST",

                    data: { values: 'Process' },
                    success: function (data) {
                        if (data == "OK") {

                            $.alert({
                                title: 'Message!',
                                content: 'You have checkout for the day Successfully',
                                type: 'green'
                            });
                            location.href = "/User/CheckIn/Process";
                        }
                        else {
                            $.alert({
                                title: 'Message!',
                                content: 'Error While CheckIn Please Logout and Log in Again',
                                type: 'red'
                            });

                        }
                    }
                });
            },
            cancel: function () {
                $.alert('Đã hủy!');
                $("#loader").hide();
            }
        }
    });

}