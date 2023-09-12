﻿$(document).ready(function () {
    const url = "/Reports/Owner_list/";
    var isfirst = true;
    LoadOwners();
    LoadData("All", "Select Status");

    $("#prntbtn").click(function () {
        $(".function-btn").hide();
        $(".filterdiv").hide();
        window.print();
        $(".function-btn").show();
        $(".filterdiv").show();
    });
    $('#ownerDropdown').change(function () {
        callajax();
    });

    $('#transStatusDropdown').change(function () {
        callajax();
    });

    $('#fromDate, #toDate').change(function () {
        callajax();
    });

    function callajax() {
        const SelectOwner = $('#ownerDropdown').val();
        const SelectStatus = $('#transStatusDropdown').val();
        const fromDate = $('#fromDate').val(); // Get selected fromDate
        const toDate = $('#toDate').val();     // Get selected toDate
        LoadData(SelectOwner, SelectStatus, fromDate, toDate);
    }
    function LoadOwners() {
        const dropdownOwner = $('#ownerDropdown');

        $.ajax({
            url,
            method: "GET",
            beforeSend: function () {
                $('#loader-overlay').show();  // Show loader before sending request
            },
            success: function (data) {
                dropdownOwner.empty();
                data.forEach(function (item) {
                    dropdownOwner.append($("<option>").val(item.OwnerName).text(item.OwnerName));
                });
                $('#loader-overlay').hide();
            }
        });
    }

    function LoadData(SelectOwner, SelectStatus, fromDate, toDate) {
        const url = "/Reports/PreviewReport/";

        $.ajax({
            url,
            data: {
                SelectOwner,
                SelectStatus,
                fromDate, // Add fromDate parameter
                toDate    // Add toDate parameter
            },
            method: "GET",
            beforeSend: function () {
                $('#loader-overlay').show();  // Show loader before sending the request
            },
            success: function (data) {
                appendData(data);
                if (!isfirst) {
                    $('#loader-overlay').hide();
                }
                isfirst = false;
            },
            error: function (xhr, status, error) {
                // Handle AJAX request error
                console.error("AJAX request error:", status, error);
            }
        });


    }

    function appendData(data) {
        const tableBody = $("#transactiontable-body");
        tableBody.empty(); // Clear existing rows

        var i = 0;
        if (data.length > 0) {
            // Use the data in your code
            data.forEach(function (item) {
                const transaction = item.Transaction;
                const barge = item.Barge;
                const transferList = item.TransferList || [];
                if (transferList.length > 0) {
                    // Create the transaction row
                    const transactionRow = $("<tr>");
                    transactionRow.append($("<td>").text(transaction?.Barge || "N/A"));
                    transactionRow.append($("<td>").text(barge?.Owner || "N/A"));
                    transactionRow.append($("<td>").text("$" + (barge?.Rate || 0) * transferList.reduce((sum, t) => sum + t.DaysIn, 0)));
                    transactionRow.append($("<td>").text(transaction?.TransactionNo));
                    transactionRow.append($("<td>").text(barge?.Size || "N/A"));
                    transactionRow.append($("<td>").text("$" + (barge?.Rate || 0)));
                    transactionRow.append($("<td>").text(transferList.reduce((sum, t) => sum + t.DaysIn, 0)));
                    transactionRow.append($("<td>").text(transaction?.Status));

                    // Append more <td> elements for other columns

                    // Append the transaction row to the table
                    tableBody.append(transactionRow);


                    const table = $("<table>").addClass("table table-bordered");

                    // Create the table header (thead) with column headers
                    const thead = $("<thead>").append(
                        $("<tr>").append(
                            $("<th>").text("From"),
                            $("<th>").text("To"),
                            $("<th>").text("Cost"),
                            $("<th colspan='3'>").text(""),
                            $("<th>").text("Days In"),
                            $("<th>").text("Status")
                        )
                    );

                    // Process transferList if needed
                    const tbody = $("<tbody>");

                    // Iterate through transferList and append rows to tbody
                    transferList.forEach(function (transfer) {
                        if (transfer.From != null && transfer.To != null) {

                            const frommilliseconds = parseInt(transfer.From.match(/\d+/)[0], 10);
                            const fromDateTime = new Date(frommilliseconds);

                            const tomilliseconds = parseInt(transfer.To.match(/\d+/)[0], 10);
                            const toDateTime = new Date(tomilliseconds);

                            const row = $("<tr>").append(
                                $("<td>").text(formatDate(fromDateTime)),
                                $("<td>").text(formatDate(toDateTime)),
                                $("<td>").text("$" + (transfer.DaysIn * (barge?.Rate || 0))),
                                $("<td colspan='3'>"),
                                $("<td>").text(transfer.DaysIn || 0),
                                $("<td>").text(transfer.Status)
                            );

                            // Append the row to tbody
                            tbody.append(row);
                        }
                    });


                    // Append the thead and tbody to the table
                    table.append(thead, tbody);

                    // Create a parent row for the transfer table
                    const transferRow = $("<tr>").append($("<td>").attr("colspan", "8").append(table));

                    // Append the transfer row to the table
                    tableBody.append(transferRow);
                    const summaryRow = $("<tr>");
                    summaryRow.append($("<td>").attr("colspan", "8").text(`Summary for Transaction# ${transaction?.TransactionNo} (${transferList.length} details Record)`));

                    // Append the summary row to the table
                    tableBody.append(summaryRow);
                }
                else {
                    i++;
                }
            });
        }
        if (i === data.length) {
            Swal.fire(
                'No Record Found',
                '',
                'warning'
            );
            const summaryRow = $("<tr>");
            summaryRow.append($("<td>").attr("colspan", "8").text("No Record Found."));
            tableBody.append(summaryRow);
        }
    }

    function formatDate(date) {
        if (!(date instanceof Date) || isNaN(date)) {
            return ''; // Return an empty string or handle the error as needed
        }

        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');
        const year = date.getFullYear();
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        const seconds = date.getSeconds().toString().padStart(2, '0');
        const ampm = date.getHours() >= 12 ? 'PM' : 'AM';

        return `${day}/${month}/${year} ${hours}:${minutes}:${seconds} ${ampm}`;
    }
});