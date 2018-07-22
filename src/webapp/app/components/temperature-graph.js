import Component from '@ember/component';
import { later } from '@ember/runloop';
import { inject } from '@ember/service';
import Chart from 'chartjs';
import moment from 'moment';

export default Component.extend({
  sensorService: inject(),
  graph: null,
  loading: false,
  empty: false,
  sensorId: null,

  didInsertElement() {
    this._super(...arguments);

    this.loadGraph();
  },

  loadGraph() {
    if(this.loading) {
      return;
    }

    this.set('loading', true);
    this.willDestroyElement();

    this.sensorService.findValues(this.sensorId, 0, 300).then(result => {
      this.set('loading', false);
      if(result.totalCount === 0) {
        this.set('empty', true);
        return;
      }

      later(() => this.initGraph(result));
    });
  },

  initGraph(result) {
    const desktop = window.matchMedia('screen and (min-width: 640px)').matches;
    let labels = [];
    let temperature = [];
    let humidity = [];

    result.items.reverse();

    if(!desktop) {
      result.items = result.items.filter((value, idx) => idx % 6 === 0);
    }

    result.items.forEach(measurement => {
      temperature.push(parseInt(measurement.temperature));
      humidity.push(parseInt(measurement.humidity));
      labels.push(moment(measurement.created).format('DD/MM HH:mm'));
    });

    let el = this.$().find('.graph');
    let config = {
      type: 'line',
      data: {
        labels: labels,
        datasets: [{
          label: ['Temperature'],
          data: temperature,
          borderColor: '#f97122',
          backgroundColor: 'rgba(255, 198, 165, 0.3)',
          yAxisID: 'y-temperature'
        }, {
          label: ['Humidity'],
          data: humidity,
          borderColor: '#2461e5',
          backgroundColor: 'rgba(163, 192, 255, 0.3)',
          yAxisID: 'y-humidity'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        legend: false,
        tooltips: {
          mode: 'point',
          callbacks: {
            title: function(tooltipItems) {
              let tooltipItem = tooltipItems[0];
              if(!tooltipItem) {
                return '';
              }

              if(tooltipItem.datasetIndex === 0) {
                return 'Temperature';
              } else if(tooltipItem.datasetIndex === 1) {
                return 'Humidity';
              }
              return '';
            },
            label: function(tooltipItem) {
              if(tooltipItem.datasetIndex === 0) {
                return tooltipItem.yLabel + ' °C';
              } else if(tooltipItem.datasetIndex === 1) {
                return tooltipItem.yLabel + '%';
              }
              return '';
            }
          }
        },
        layout: {
          padding: {
            left: desktop ? 10 : 0,
            right: desktop ? 10 : 0,
            top: 15,
            bottom: 10
          }
        },
        scales: {
          xAxes: [{
            display: true
          }],
          yAxes: [{
            type: 'linear',
            display: desktop,
            position: 'right',
            id: 'y-humidity',
            ticks: {
              suggestedMin: 0,
              suggestedMax: 100,
              callback: function(value) {
                return value + '%';
              }
            }
          }, {
            type: 'linear',
            fill: false,
            display: true,
            position: 'left',
            id: 'y-temperature',
            ticks: {
              suggestedMin: 0,
              suggestedMax: 20,
              callback: function(value) {
                return value + ' °C';
              }
            },
            gridLines: {
              drawOnChartArea: false
            }
          }]
        }
      }
    };

    // Expertly deduced numbers
    const chromeHeight = 200;
    const itemHeight = 15;

    el.css({ height: (chromeHeight + labels.length * itemHeight) + 'px' });
    let graph = new Chart(el[0], config);
    this.set('graph', graph);
  },

  willDestroyElement() {    
    if(!!this.graph) {
      this.graph.destroy();
    }
  }
});
