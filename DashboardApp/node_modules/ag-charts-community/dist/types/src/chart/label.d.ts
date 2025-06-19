import { type RequireOptional } from 'ag-charts-core';
import type { AgChartLabelFormatterParams, AgChartLabelOptions, FontStyle, FontWeight, Formatter, TimeIntervalUnit } from 'ag-charts-types';
import type { Scale } from '../scale/scale';
import type { Matrix } from '../scene/matrix';
import type { PlacedLabelDatum } from '../scene/util/labelPlacement';
import { BaseProperties } from '../util/properties';
import { type TextMeasurer } from '../util/textMeasurer';
import type { TimeInterval } from '../util/time';
import type { ChartAxisLabel, ChartAxisLabelFlipFlag } from './chartAxis';
export declare class Label<TParams = never, TDatum = any> extends BaseProperties implements AgChartLabelOptions<TDatum, RequireOptional<TParams>> {
    enabled: boolean;
    color?: string;
    fontStyle?: FontStyle;
    fontWeight?: FontWeight;
    fontSize: number;
    fontFamily: string;
    formatter?: Formatter<AgChartLabelFormatterParams<TDatum> & RequireOptional<TParams>>;
    getFont(): string;
}
export declare function calculateLabelRotation(rotation?: number, parallel?: boolean, regularFlipRotation?: number, parallelFlipRotation?: number): {
    configuredRotation: number;
    defaultRotation: number;
    parallelFlipFlag: ChartAxisLabelFlipFlag;
    regularFlipFlag: ChartAxisLabelFlipFlag;
};
export declare function getLabelSpacing(minSpacing?: number, rotated?: boolean): number;
export declare function getTextBaseline(parallel: boolean, labelRotation: number, sideFlag: ChartAxisLabelFlipFlag, parallelFlipFlag: ChartAxisLabelFlipFlag, backwardsCompatibleTopBaseline: boolean): CanvasTextBaseline;
export declare function getTextAlign(parallel: boolean, labelRotation: number, labelAutoRotation: number, sideFlag: ChartAxisLabelFlipFlag, regularFlipFlag: ChartAxisLabelFlipFlag): CanvasTextAlign;
export declare function labelSpecifier(format: ChartAxisLabel['format'] | undefined, timeInterval: TimeInterval | TimeIntervalUnit | undefined): string | undefined;
export declare function timeIntervalMaxLabelSize(scale: Scale<Date, number>, label: ChartAxisLabel, primaryLabel: ChartAxisLabel | undefined, domain: Date[], ticks: Date[], timeInterval: TimeInterval | TimeIntervalUnit, textMeasurer: TextMeasurer): {
    width: number;
    height: number;
};
export declare function createLabelData(tickData: {
    tickLabel: string | undefined;
    translationY: number;
}[], labelX: number, labelMatrix: Matrix, textMeasurer: TextMeasurer): PlacedLabelDatum[];
export declare function createFixedLabelData({ width, height, spacing }: {
    width: number;
    height: number;
    spacing: number;
}, labelX: number, labelMatrix: Matrix): PlacedLabelDatum[];
