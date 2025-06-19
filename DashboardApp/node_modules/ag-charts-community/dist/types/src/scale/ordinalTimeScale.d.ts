import type { TimeInterval } from '../util/time';
import { DiscreteTimeScale } from './discreteTimeScale';
import type { NormalizedDomain, ScaleFormatParams, ScaleTickParams, ScaleTickResult } from './scale';
export declare class OrdinalTimeScale extends DiscreteTimeScale {
    readonly type = "ordinal-time";
    static is(value: unknown): value is OrdinalTimeScale;
    private _domain;
    private sortedTimestamps;
    private isReversed;
    private precomputedSteps;
    set domain(domain: Date[]);
    get domain(): Date[];
    private _bands;
    get bands(): Date[];
    normalizeDomains(...domains: Date[][]): NormalizedDomain<Date>;
    ticks({ interval, maxTickCount }: ScaleTickParams<TimeInterval | number>, domain?: Date[], visibleRange?: [number, number], extend?: boolean): ScaleTickResult<Date> | undefined;
    private getSortedTimestamps;
    private getPrecomputedSteps;
    private findIntervalInRange;
    private findInterval;
    datumFormatter(params: ScaleFormatParams<Date>): (date: Date) => string;
    findIndex(value: Date): number | undefined;
}
