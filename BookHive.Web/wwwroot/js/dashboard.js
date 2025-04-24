var chart;

$(document).ready(function () {
    drawSubscribersChart();
    drawRentalsCharts();
    const targetNode = document.getElementById("DateRange");

    const observer = new MutationObserver(function (mutationsList, observer) {
        // Each mutation record could be useful, but for simple cases:
        const selectedRange = $("#DateRange").html();

        var dataRange = selectedRange.split(' - ');
        chart.destroy();
        console.log(dataRange);
        drawRentalsCharts(dataRange[0], dataRange[1]);
    });

    observer.observe(targetNode, {
        childList: true,
        subtree: true,
        characterData: true
    });
});


function drawRentalsCharts(startDate=null,endDate=null) {
    var element = document.getElementById('RentalsPerDay');

    var height = parseInt(KTUtil.css(element, 'height'));
    var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
    var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
    var baseColor = KTUtil.getCssVariableValue('--kt-info');
    var lightColor = KTUtil.getCssVariableValue('--kt-info-light');

    if (!element) {
        return;
    }

    $.get({
        url: `Dashboard/GetRentalsPerDay?StartDay=${startDate}&EndDate=${endDate}`,
        success: function (data) {
            console.log(data);
            var options = {
                series: [{
                    name: 'Net Profit',
                    data: data.map(i => i.value)
                }],
                chart: {
                    fontFamily: 'inherit',
                    type: 'area',
                    height: height,
                    toolbar: {
                        show: false
                    }
                },
                plotOptions: {

                },
                legend: {
                    show: false
                },
                dataLabels: {
                    enabled: false
                },
                fill: {
                    type: 'solid',
                    opacity: 1
                },
                stroke: {
                    curve: 'smooth',
                    show: true,
                    width: 3,
                    colors: [baseColor]
                },
                xaxis: {
                    categories: data.map(i => i.label),
                    axisBorder: {
                        show: false,
                    },
                    axisTicks: {
                        show: false
                    },
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    },
                    crosshairs: {
                        position: 'front',
                        stroke: {
                            color: baseColor,
                            width: 1,
                            dashArray: 3
                        }
                    },
                    tooltip: {
                        enabled: true,
                        formatter: undefined,
                        offsetY: 0,
                        style: {
                            fontSize: '12px'
                        }
                    }
                },
                yaxis: {
                    tickAmount: Math.max(...data.map(d => d.value)),
                    min:0,
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }
                },
                states: {
                    normal: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    hover: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    active: {
                        allowMultipleDataPointsSelection: false,
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    }
                },
                tooltip: {
                    style: {
                        fontSize: '12px'
                    },
                    y: {
                        formatter: function (val) {
                            return '$' + val + ' thousands'
                        }
                    }
                },
                colors: [lightColor],
                grid: {
                    borderColor: borderColor,
                    strokeDashArray: 4,
                    yaxis: {
                        lines: {
                            show: true
                        }
                    }
                },
                markers: {
                    strokeColor: baseColor,
                    strokeWidth: 3
                }
            };

            chart = new ApexCharts(element, options);
            chart.render();
        }
    });

   
}

function drawChart() {
    var ctx = document.getElementById('SubscribersPerCity');

    // Define colors
    var primaryColor = KTUtil.getCssVariableValue('--kt-primary');
    var dangerColor = KTUtil.getCssVariableValue('--kt-danger');
    var successColor = KTUtil.getCssVariableValue('--kt-success');
    var warningColor = KTUtil.getCssVariableValue('--kt-warning');
    var infoColor = KTUtil.getCssVariableValue('--kt-info');

    // Define fonts
    var fontFamily = KTUtil.getCssVariableValue('--bs-font-sans-serif');

    // Chart labels
    const labels = ['January', 'February', 'March', 'April', 'May'];

    // Chart data
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Monthly Sales',
                data: [120, 90, 150, 80, 70], // one value per label
                backgroundColor: [
                    primaryColor,
                    dangerColor,
                    successColor,
                    warningColor,
                    infoColor
                ],
                borderColor: '#fff',
                borderWidth: 2
            } // <-- you were missing this closing brace!
        ]
    };

    // Chart config
    const config = {
        type: 'pie',
        data: data,
        options: {
            plugins: {
                title: {
                    display: false,
                }
            },
            responsive: true,
        },
        defaults: {
            global: {
                defaultFont: fontFamily
            }
        }
    };

    // Init ChartJS -- for more info, please visit: https://www.chartjs.org/docs/latest/
    var myChart = new Chart(ctx, config);


}


function drawSubscribersChart() {

    $.get({
        url: '/Dashboard/GetSubscribersPerCity',
        success: function (figures) {
            var ctx = document.getElementById('SubscribersPerCity');

            // Define colors
            var primaryColor = KTUtil.getCssVariableValue('--kt-primary');
            var dangerColor = KTUtil.getCssVariableValue('--kt-danger');
            var successColor = KTUtil.getCssVariableValue('--kt-success');
            var warningColor = KTUtil.getCssVariableValue('--kt-warning');
            var infoColor = KTUtil.getCssVariableValue('--kt-info');

            // Define fonts
            var fontFamily = KTUtil.getCssVariableValue('--bs-font-sans-serif');

            // Chart data
            const data = {
                labels: figures.map(f => f.label),
                datasets: [{
                    data: figures.map(f => f.value),
                    backgroundColor: [
                        infoColor,
                        successColor,
                        warningColor,
                        primaryColor,
                        dangerColor,
                        '#5F91B6',
                        '#D3F6FC',
                        '#C8B0D2'
                    ],
                    borderRadius: 8
                }]
            };

            // Chart config
            const config = {
                type: 'doughnut',
                data: data,
                options: {
                    plugins: {
                        title: {
                            display: false,
                        }
                    },
                    responsive: true,
                },
                defaults: {
                    global: {
                        defaultFont: fontFamily
                    }
                }
            };

            new Chart(ctx, config);
        }
    });
}



