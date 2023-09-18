$(document).ready(function () {
    const url = "/Reports/Owner_list/";
    var isfirst = true;
    LoadOwners();
    LoadBarges("All");

    $("#prntbtn").click(function () {
        $(".function-btn").hide();
        $(".filterdiv").hide();
        window.print();
        $(".function-btn").show();
        $(".filterdiv").show();
    });

    $('#ownerDropdown').change(function () {
        const SelectOwner = $('#ownerDropdown').val();
        LoadBarges(SelectOwner);
    });
    $('#exportBtn').click(function (event) {
        event.preventDefault(); // Prevent the default form submission behavior

        $.ajax({
            url: '/Maintenance/ExportBargesToExcel', // Make sure the URL is correct
            type: 'GET',
            xhrFields: {
                responseType: 'blob' // Set the response type to blob
            },
            success: function (blob) {
                // Save the blob as a file using FileSaver.js
                saveAs(blob, 'Barges.xlsx');
            },

            error: function (error) {
                console.error("Error exporting data: ", error);
            }
        });
    });


    $('#importBtn').click(function (event) {
        event.preventDefault();
        // Prevent the default form submission behavior
        // Create a hidden file input element
        var fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.accept = '.xlsx, .xls';
        fileInput.style.display = 'none';

        // Attach change event handler to the hidden input
        fileInput.addEventListener('change', function () {
            var file = fileInput.files[0];

            if (file) {
                var formData = new FormData();
                formData.append('excelFile', file);

                // Start the import process
                importBarges(formData);
            }
        });

        // Trigger click on the hidden file input
        fileInput.click();
    });
    function importBarges(formData) {
        $.ajax({
            url: '/Maintenance/CheckDuplicateBarge',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function () {
                $('#loader-overlay').show();  // Show loader before sending request
            },
            success: function (response) {
                $('#loader-overlay').hide();
                if (response.isDuplicate) {
                    handleDuplicateBarges(response, formData);
                } else {
                    proceedWithImport(formData);
                }
            },
            error: function (error) {
                $('#loader-overlay').hide();
                alert("Error impoting Barges: " + error.responseText);
            }
        });
    }

    function proceedWithImport(formData) {
        $.ajax({
            url: '/Maintenance/ImportBargesFromExcel', // Server URL for import
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function () {
                $('#loader-overlay').show(); // Show loader before sending request
            },
            success: function (response) {
                $('#loader-overlay').hide();
                // Handle successful import response
                alert("Import successful:", response);
            },
            error: function (error) {
                $('#loader-overlay').hide();
                // Show an alert for other errors
                Swal.fire(
                    'Error importing data:',
                    '\n' + error.responseText,
                    'error'
                );
            }
        });
    }
    function handleDuplicateBarges(response, formData) {
        if (response.totalnewbarges > 0) {
            Swal.fire({
                title: response.totalnewbarges + ' New Barges in this file.\n' + response.totalduplicatebarge + ' Duplicate Barges Found.',
                text: 'Do you want to skip duplicate barges and import or cancel importing?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Skip Duplicates and Import',
                cancelButtonText: 'Cancel Importing'
            }).then((result) => {
                if (result.isConfirmed) {
                    proceedWithImport(formData);
                }
                else {
                    Swal.fire(
                        'Cancelled',
                        'Import process canceled.',
                        'error'
                    );
                }
            })
        }
        else {
            Swal.fire(
                'No Additional Barges',
                'All Barges are already Stored. No New Barge Found in this Excel File.',
                'warning'
            );
        }
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
                setTimeout(function () {
                    $('#loader-overlay').hide();
                }, 3000);
            }
        });
    }
    function LoadBarges(SelectOwner) {        
        const url = "/Reports/BargeByOwner/";

        $.ajax({
            url,
            data: { SelectOwner },
            method: "GET",
            beforeSend: function () {
                $('#loader-overlay').show();  // Show loader before sending request
            },
            success: function (data) {
                const tableBody = $("#table-body");
                tableBody.empty(); // Clear existing rows

                data.forEach(function (item) {
                    const row = $("<tr>");
                    row.append($("<td>").text(item.Barge_Name));
                    row.append($("<td>").text(item.Size));
                    row.append($("<td>").text(item.Rate));
                    row.append($("<td>").text(item.Owner));
                    row.append($("<td>").text(item.Description));
                    // Append more <td> elements for other columns

                    tableBody.append(row);
                });
                if (!isfirst) {
                    setTimeout(function () {
                        $('#loader-overlay').hide();
                    }, 3000);
                }     
                isfirst = false;
            }
        });
    }
});
