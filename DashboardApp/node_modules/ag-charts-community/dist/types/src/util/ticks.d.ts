import { type TimeInterval } from './time/interval';
interface TickInterval {
    duration: number;
    timeInterval: TimeInterval;
    step: number;
}
export declare const TickIntervals: TickInterval[];
export declare function createTicks(start: number, stop: number, count: number, minCount?: number, maxCount?: number, visibleRange?: [number, number]): {
    ticks: number[];
    count: number;
};
export declare function getTickTimeInterval(start: number, stop: number, count: number, minCount: number | undefined, maxCount: number | undefined, { weekStart, primaryOnly, targetInterval, }: {
    weekStart: TimeInterval | undefined;
    primaryOnly?: boolean;
    targetInterval?: number;
}): TimeInterval | undefined;
export declare function tickStep(start: number, end: number, count: number, minCount?: number, maxCount?: number): number;
export declare function tickFormat(ticks: any[], format?: string): (n: number | {
    valueOf(): number;
}) => string;
export declare function range(start: number, end: number, step: number, visibleRange?: [number, number]): {
    ticks: number[];
    count: number;
};
export declare function isDenseInterval(count: number, availableRange: number): boolean;
export declare function niceTicksDomain(start: number, end: number): number[];
export declare function estimateTickCount(rangeExtent: number, zoomExtent: number, minSpacing: number | undefined, maxSpacing: number | undefined, defaultTickCount: number, defaultMinSpacing: number): {
    minTickCount: number;
    maxTickCount: number;
    tickCount: number;
};
export {};
