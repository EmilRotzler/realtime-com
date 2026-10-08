<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
  type ChartData,
  type ChartOptions,
} from 'chart.js'
import type { MinuteCount } from '@/types'

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip)

const props = defineProps<{ stats: MinuteCount[] }>()

const timeFormat = new Intl.DateTimeFormat(undefined, {
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

const currentCount = computed(() => props.stats.at(-1)?.count ?? 0)

const chartData = computed<ChartData<'bar'>>(() => ({
  labels: props.stats.map((entry) => timeFormat.format(new Date(entry.minute))),
  datasets: [
    {
      label: 'Messages',
      data: props.stats.map((entry) => entry.count),
      // The current (still growing) minute stands out
      backgroundColor: props.stats.map((_, i) => (i === props.stats.length - 1 ? '#42b883' : '#a8d5c2')),
    },
  ],
}))

const options: ChartOptions<'bar'> = {
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 200 },
  scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
}
</script>

<template>
  <section class="message-chart">
    <header>
      <h2>Messages per minute</h2>
      <span v-if="stats.length" data-testid="current-minute-count">This minute: {{ currentCount }}</span>
    </header>
    <div class="canvas-wrap">
      <Bar :data="chartData" :options="options" />
    </div>
  </section>
</template>

<style scoped>
header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}
h2 {
  font-size: 1rem;
  margin: 0;
}
.canvas-wrap {
  position: relative;
  height: 200px;
}
</style>
