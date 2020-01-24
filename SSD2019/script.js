//DOM MANIPULATION 
$(document).ready(function () {

    var ip = "10.211.55.4";
    var protocol = "https"; //https
    var port = "44385"; //  52436
    var separator = "\n\n\n--------------------------------------------------------------------\n\n\n";
    //UTILITY FUNCTIONS
    function getSelectedOptimizationMethod() {
        return $(".optimizationMethod").children("option:selected").text();
    }
    function appendTextAreaResults(newResult) {
        var text = $("#resultsTextArea").val();
        $("#resultsTextArea").val(text + "\n" + separator + "\n" + newResult);
        $('#resultsTextArea').scrollTop($('#resultsTextArea')[0].scrollHeight);
    }

    var cust = null;



    //SERVER INTERACTION FUNCTIONS
    function readAll() {
        $.ajax(
            {
                url: protocol+"://"+ip+":"+port+"/api/orders",
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType : "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    var prova = "";
                    for (var order in result) {
                        prova += result[order].customer + " quant: " + result[order].quant + "\n";
                    }
                    appendTextAreaResults(prova);
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function readCustomerOrders(customer) {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/customers/"+customer+"/orders",
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    var prova = "";
                    var totQuant = 0;
                    for (var order in result) {
                        totQuant += result[order].quant;
                    }
                    prova = result[order].customer + " total quant: " + totQuant + "\n";
                    appendTextAreaResults(prova);
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function deleteAllCustomerOrders(customer) {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/customers/" + customer + "/orders",
                type: "DELETE",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    appendTextAreaResults("Customer " + customer + " deleted.");
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function resetCustomerOrdersQuant(customer) {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/customers/" + customer + "/orders",
                type: "PUT",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    appendTextAreaResults("Quant of customer " + customer + " resetted.")
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function getAllOrdersChart() {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/ordersChart",
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    $("#graphic").attr("src", "data:image/png;base64," + result);
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function forecastsSpecifiedCustomer(customer) {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/customers/"+customer+"/forecasts",
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    $("#graphic").attr("src", "data:image/png;base64," + result);
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function forecastsAll() {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port +"/api/forecasts",
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    var prova = "";
                    for (var order in result) {
                        prova += result[order].customer + " :\n " + result[order].forecasts + "\n";
                    }
                    appendTextAreaResults(prova);
 
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    function optimize(optimizeMethod) {
        $.ajax(
            {
                url: protocol + "://" + ip + ":" + port + "/api/" + optimizeMethod,
                type: "GET",
                contentType: "application/json",
                data: "",
                dataType: "json",
                beforeSend: function (xhr) {
                    $('#overlay').fadeIn(300);
                },
                complete: function (xhr, status) {
                    setTimeout(function () {
                        $('#overlay').fadeOut(300);
                    }, 500);
                },
                success: function (result) {
                    appendTextAreaResults(optimizeMethod + ": "+JSON.stringify(result));
                },
                error: function (xhr, status, p3, p4) {
                    alert("Something went wrong");
                }
            });
    }
    

    
    $('#getAll').click(function () {
        readAll();
    })
    $('#getAllForCustomer').click(function () {
        readCustomerOrders(cust);
    })
    $('#deleteOrdersForCustomer').click(function () {
        deleteAllCustomerOrders(cust);
    })
    $('#resetCustomerQuant').click(function () {
        resetCustomerOrdersQuant(cust);
    })
    $('#chartCustomers').click(function () {
        getAllOrdersChart(cust);
    })
    $('#forecast').click(function () {
        if ($('input').val().length > 0) {
            forecastsSpecifiedCustomer(cust);
        } else {
            forecastsAll();
        }
    })
    $('#optimize').click(function () {
        optimize(getSelectedOptimizationMethod());
    })


    $('.customerButton').prop("disabled", true);
    $('input').keyup(function () {
        if ($(this).val().length > 0) {
            cust = $(this).val();
            $('.customerButton').removeClass('buttonNoHover');
            $('.customerButton').addClass('buttonHover');
            $('.customerButton').prop("disabled", false);
            $('#addOrderForCustomer').prop("disabled", true);
            $('#addOrderForCustomer').removeClass('buttonHover');
            $('#addOrderForCustomer').addClass('buttonNoHover');

        } else {
            cust = null;
            $('.customerButton').removeClass('buttonHover');
            $('.customerButton').addClass('buttonNoHover');
            $('.customerButton').prop("disabled", true);
        }
    });

});
