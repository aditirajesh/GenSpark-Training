import type { TimeIntervalUnit } from 'ag-charts-types';
import type { Scale } from '../../scale/scale';
import { type AxisPrimaryTickCount } from '../../util/secondaryAxisTicks';
import { TimeInterval } from '../../util/time';
import type { ChartAxis, ChartAxisLabelFlipFlag } from '../chartAxis';
import type { AxisInterval } from './axisInterval';
import type { TickInterval } from './axisTick';
import { NiceMode, type TickDatum } from './axisUtil';
export interface TickData<D = any> {
    tickDomain: D[];
    rawTicks: D[];
    rawTickCount: number | undefined;
    fractionDigits: number;
    ticks: TickDatum[];
    timeInterval: TimeInterval | TimeIntervalUnit | undefined;
    niceDomain?: D[];
}
export interface TickGenerationParams<D = any> {
    domain: D[];
    primaryTickCount: AxisPrimaryTickCount | undefined;
    visibleRange: [number, number];
    niceMode: NiceMode;
    parallelFlipRotation: number;
    regularFlipRotation: number;
    labelX: number;
    sideFlag: ChartAxisLabelFlipFlag;
    removeOverflowLabels: boolean;
    removeOverflowThreshold?: number;
}
export interface TickGenerationResult<D = any> {
    tickData: TickData<D>;
    rotation: number;
    textBaseline: CanvasTextBaseline;
    textAlign: CanvasTextAlign;
}
export interface TickGenerationAxis<S extends Scale<D, number, TickInterval<S>>, D> {
    readonly range: [number, number];
    readonly reverse: boolean;
    readonly scale: S;
    readonly label: ChartAxis['label'];
    readonly primaryLabel?: ChartAxis['label'];
    readonly interval: AxisInterval<S>;
    readonly defaultTickMinSpacing: number;
    readonly inRange: ChartAxis['inRange'];
    formatTick(value: any, index: number, domain: D[], fractionDigits?: number, timeInterval?: TimeInterval | TimeIntervalUnit, formatter?: (datum: any) => string): string;
}
export declare class AxisTickGenerator<S extends Scale<D, number, TickInterval<S>>, D> {
    private readonly axis;
    constructor(axis: TickGenerationAxis<S, D>);
    private estimateTickCount;
    private filterTicks;
    generateTicks({ domain, primaryTickCount, visibleRange, niceMode, parallelFlipRotation, regularFlipRotation, labelX, sideFlag, removeOverflowLabels, removeOverflowThreshold, }: TickGenerationParams<D>): TickGenerationResult<D>;
    private getTickStrategies;
    private ticksEqual;
    private createTickData;
    private getTimeIntervalTicks;
    private timeScaleTicks;
    private getTicks;
}
