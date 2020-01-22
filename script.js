
//DOM MANIPULATION 
$(document).ready(function () {
    //UTILITY FUNCTIONS
    function getSelectedOptimizationMethod() {
        return $(".optimizationMethod").children("option:selected").text();
    }
    var cust = null;

    //SERVER INTERACTION FUNCTIONS
    function readAll() {
        $.ajax(
            {
                url: "localhost:52436/api/orders",
                type: "GET",
                contentType: "application/json",
                data: "",
                success: function (result) {
                    alert(JSON.stringify(result));//TODO stringify
                    $("#resultsTextArea").val(JSON.stringify(result));
                },
                error: function (xhr, status, p3, p4) {
                    alert(xhr, status);
                }
            });
    }
    function readCustomerOrders(customer) {
        //richiesta al server
        alert("you want to read all the orders of the customer " + customer);
    }

    $('#getAll').click(function () {
        readAll();
    })
    $('#getAllForCustomer').click(function () {
        readCustomerOrders(cust);
    })
    $('#deleteOrdersForCustomer').click(function () {

    })
    $('#resetCustomerQuant').click(function () {

    })
    $('#forecast').click(function () {

    })
    $('#optimize').click(function () {
        var selectedMethod = getSelectedOptimizationMethod();
        alert("You have selected - " + selectedMethod);
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
