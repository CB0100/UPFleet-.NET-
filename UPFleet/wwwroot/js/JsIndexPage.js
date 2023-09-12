﻿

$(document).ready(function () {

    //getting all dropdowns first option..
    var firstBargeOption = $('#bargeDropdown').find("option").eq(0);
    var firstStatusOption = $('#statusDropdown').find("option").eq(0);
    var StatusOption2 = $('#statusDropdown_2').find("option").eq(0);
    var FromLocationOption = $('#Fromlocationdropdown').find("option").eq(0);
    var ToLocationOption = $('#Tolocationdropdown').find("option").eq(0);


    checkTranactioninputSelection();

    // Making rate input to the formatted form like 0.00
    $("#rateInput").val(parseFloat($("#rateInput").val()).toFixed(2));

    $('#bargeDropdown').on('input',
        function () {
            const selectedBarge = $(this).val();
            var selectedStatus = $('#statusDropdown').val();
            if (selectedBarge !== '') {
                // Send an AJAX request to retrieve the details for the selected barge and status
                $.ajax({
                    url: '/Maintenance/GetBargeDetails',
                    type: 'GET',
                    data: { barge: selectedBarge, status: selectedStatus },
                    success: function (response) {
                        // Update the fields with the retrieved data
                        $('#rateInput').val(parseFloat(response.Rate).toFixed(2));
                        $('#ownerInput').val(response.Owner);
                        $('#transactionInput').val(response.Transaction);
                        checkTranactioninputSelection();
                        $('#Cancelbtn').hide();
                    },
                    error: function () {
                        console.log('Error occurred while retrieving barge details.');
                    }
                });
            } else {
                // Clear the fields if no barge is selected
                clearFields();
            }
        });
    var rowCount = 1;

    $('#addRowButton').on('click', function () {
        // cloning first row of the table
        var newRow = $('#dynamicTable tbody tr:first').clone();

        //clearing the new row fields
        newRow.find('input').val('');

        //selecting the first option in the new row status dropdown
        newRow.find('select').prop('selectedIndex', 0);
        newRow.find('input').each(function () {
            var nameAttribute = $(this).attr('name');
            nameAttribute = nameAttribute.replace('[0]', '[' + rowCount + ']');
            $(this).attr('name', nameAttribute);
        });

        $('#dynamicTable tbody').append(newRow);

        rowCount++;
    });
    $('#Cancelbtn').on('click', function () {
        location.reload();
    });
    $('#addTransactionButton').on('click',
        function () {
            // Clear all fields
            firstBargeOption.prop('selected', true);
            firstStatusOption.prop('selected', true);

            clearFields();

            // Remove all rows except the first one
            $('#dynamicTable tbody tr:not(:first)').remove();

            // Clear input fields in the first row
            $('#dynamicTable tbody tr:first').find('input, select').val('');

            StatusOption2.prop('selected', true);
            FromLocationOption.prop('selected', true);
            ToLocationOption.prop('selected', true);
            // Hide the partial view and buttons
            $('#transfersGrid,  #deleteButton,  #updateButton, #savetransfer').hide();

            // Show the Add New Transaction button
            $(this).hide();
            $('#Cancelbtn').show();

            $('#bargeDropdown').prop('disabled', false);
        });

    function checkTranactioninputSelection() {
        var selectedValue = $('#transactionInput').val();

        if (selectedValue !== '') {
            $('#savetransfer').show();
            $('#updateButton').show();
            $('#deleteButton').show();
            $('#addTransactionButton').show();
            $('#Cancelbtn').hide();

            $('#bargeDropdown').prop('disabled', true);
        } else {
            $('#savetransfer').hide();
            $('#deleteButton').hide();
            $('#updateButton').hide();
        }
    }
    function clearFields() {
        $('#rateInput').val('0.00');
        $('#ownerInput').val('');
        $('#transactionInput').val('');
    }

    $('#transferForm').submit(function () {
        // Update the name attribute of the status element in each row
        $('#dynamicTable tbody tr').each(function (index) {
            var inputs = $(this).find('input, select');
            inputs.each(function () {
                var nameAttribute = $(this).attr('name');
                nameAttribute = nameAttribute.replace(/\[\d+\]/, '[' + index + ']');
                $(this).attr('name', nameAttribute);
            });
        });

    });

    $('#deleteButton').click(function () {
        // Get the input value
        var inputValue = $('#transactionInput').val();
        var barge = $('#bargeDropdown').val();

        // Show a confirmation dialog
        Swal.fire({
            title: 'Are you sure you want to Delete This Transaction?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({
                    title: 'Deleted!',
                    text: 'Transaction has been deleted.',
                    icon: 'success',
                    showConfirmButton: false
                });
                $.ajax({
                    url: '/Maintenance/Delete_transaction',
                    type: 'GET',
                    data: { transactionInput: inputValue },
                    success: function (response) {
                        window.location.href = '/Home/IndexPage?BargeName=' + barge;
                    },
                    error: function () {
                        console.log('Error occurred while retrieving barge details.');
                    }
                });
            }
        })
    });

    $('#updateButton').click(function () {
        // Get the input value
        var inputValue = $('#transactionInput').val();
        var barge = $('#bargeDropdown').val();
        var rate = $('#rateInput').val();
        var status = $('#statusDropdown').val();
        // Create an AJAX request
        if (inputValue !== "") {
            $.ajax({
                url: '/Maintenance/Update_transaction',
                type: 'GET',
                data: { transactionInput: inputValue, status: status, Rate: rate },
                success: function (response) {

                    if (response.currentTransactionType === 'Update') {
                        Swal.fire({
                            title: 'Updated',
                            text: 'Transaction Updated Successfully',
                            icon: 'success',
                            showConfirmButton: false
                        });
                        window.location.href = '/Home/IndexPage?Transactionno=' + inputValue;
                    }
                    else {
                        Swal.fire({
                            title: 'Updated',
                            text: 'Transaction Updated Successfully',
                            icon: 'success'
                        });
                    }
                },
                error: function () {
                    console.log('Error occurred while retrieving barge details.');
                }
            });
        }
    });
    $('#rateInput').keypress(function (e) {
        var key = e.key;
        var regex = /[0-9.]/;

        var currentValue = e.target.value;
        var decimalIndex = currentValue.indexOf('.');
        var currentValuelength = currentValue.length;


        // Allow only digits and a single decimal point
        if (!regex.test(key)) {
            e.preventDefault();
        }

        // Allow only one decimal point
        if (key === '.' && decimalIndex !== -1) {
            e.preventDefault();
        }

        // Allow only two digits after the decimal point
        if (decimalIndex !== -1 && currentValue.substring(decimalIndex + 1).length >= 2) {
            // Check if the cursor position is after the decimal point
            var cursorPosition = e.target.selectionStart;
            if (cursorPosition > decimalIndex) {
                e.preventDefault();
            }
        }
        if (currentValuelength === 0 && key === '.') {
            e.preventDefault();
        }
    });

});