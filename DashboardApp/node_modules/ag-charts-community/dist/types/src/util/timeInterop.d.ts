import type { TimeIntervalUnit } from 'ag-charts-types';
import * as time from './time';
import type { TimeInterval } from './time/interval';
export declare function intervalInstance(interval: TimeInterval | TimeIntervalUnit): TimeInterval;
export declare function intervalUnit(interval: TimeInterval | TimeIntervalUnit): TimeIntervalUnit;
export declare function intervalStep(interval: TimeInterval | TimeIntervalUnit): number;
export declare function intervalHierarchy(interval: TimeInterval | TimeIntervalUnit): time.TimeInterval | undefined;
export declare function intervalMilliseconds(interval: TimeInterval | TimeIntervalUnit): number;
export declare function intervalRange(interval: TimeInterval | TimeIntervalUnit, start: Date, stop: Date, options?: {
    visibleRange?: [number, number];
    extend?: boolean;
    limit?: number;
}): Date[];
export declare function intervalFloor(interval: TimeInterval | TimeIntervalUnit, date: Date): Date;
export declare function intervalCeil(interval: TimeInterval | TimeIntervalUnit, date: Date): Date;
