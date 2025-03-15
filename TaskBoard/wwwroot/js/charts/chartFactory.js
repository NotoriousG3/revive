let chartColors = [
    '#16a085',
    '#3498db',
    '#9b59b6',
    '#34495e',
    '#f1c40f',
    '#f39c12',
    '#d35400',
    '#e74c3c',
    '#78281f',
    '#515a5a',
    '#2874a6',
    '#16a085',
    '#a2d9ce',
    '#73c6b6',
    '#45b39d',
    '#76448a',
    '#f4d03f',
    '#d68910',
    '#ba4a00',
    '#e74c3c',
    '#58d68d',
    '#5dade2',
    '#884ea0',
    '#d4ac0d',
    '#99a3a4',
    '#0b5345',
    '#85c1e9',
    '#9b59b6',
    '#1b4f72',
    '#85929e',
    '#f7dc6f',
    '#d35400',
    '#1d8348',
    '#2e86c1',
    '#af7ac5',
    '#e67e22',
    '#9a7d0a',
    '#a04000',
    '#cb4335',
    '#3498db',
    '#8e44ad',
    '#28b463',
    '#21618c',
    '#c39bd3',
    '#512e5f',
    '#f9e79f',
    '#fef9e7',
    '#f5b041',
    '#dc7633',
    '#6e2c00',
    '#c0392b',
    '#82e0aa',
    '#138d75',
    '#aed6f1',
    '#2980b9',
    '#d7bde2',
    '#117a65',
    '#633974',
    '#0e6655',
    '#2ecc71',
    '#abebc6',
    '#2ecc71',
    '#239b56',
    '#186a3b',
    '#27ae60',
    '#aeb6bf',
    '#5d6d7e',
    '#34495e',
    '#2c3e50',
    '#f1c40f',
    '#b7950b',
    '#7d6608',
    '#fad7a0',
    '#f8c471',
    '#f39c12',
    '#b9770e',
    '#9c640c',
    '#7e5109',
    '#edbb99',
    '#e59866',
    '#873600',
    '#f1948a',
    '#ec7063',
    '#b03a2e',
    '#943126',
    '#7f8c8d',
    '#7f8c8d',
    '#707b7c',
    '#616a6b',
    '#424949'
];

Apex.colors = chartColors;

function CreateBarChart(selector, title, height, series, extraOptions = {}) {
    let options = {
        chart: {
            height: height,
            type: 'bar',
            foreColor: 'white'
        },
        title: {
            text: title,
            align: 'center',
        },
        dataLabels: {
            enabled: true,
            offsetX: -6,
            style: {
                fontSize: '12px',
                colors: ['white']
            }
        },
        stroke: {
            show: false
        },
        plotOptions: {
            bar: {
                distributed: true
            }
        },
        legend: {
            show: false
        },
        series: series,
        xaxis: {
            type: 'category'
        }
    };
    
    let finalOptions = Object.assign({}, options, extraOptions);
    let apexBarChart = new ApexCharts(document.querySelector(selector), finalOptions);
    apexBarChart.render();
}

function CreatePieChart(selector, title, height, series, labels, extraOptions = {}) {
    let options = {
        chart: {
            height: height,
            type: 'pie',
            foreColor: 'white'
        },
        title: {
            text: title,
            align: 'center',
        },
        dataLabels: {
            enabled: true,
            offsetX: -6,
            style: {
                fontSize: '12px',
                colors: ['white']
            }
        },
        stroke: {
            show: false
        },
        legend: {
            show: false
        },
        series: series,
        labels: labels
    };

    let finalOptions = Object.assign({}, options, extraOptions);
    let apexBarChart = new ApexCharts(document.querySelector(selector), finalOptions);
    apexBarChart.render();
}