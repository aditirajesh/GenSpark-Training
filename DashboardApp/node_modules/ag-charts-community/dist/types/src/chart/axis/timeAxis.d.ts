import type { ModuleContext } from '../../module/moduleContext';
import { TimeScale } from '../../scale/timeScale';
import { BaseProperties } from '../../util/properties';
import { AxisLabel } from './axisLabel';
import { AxisTick } from './axisTick';
import { CartesianAxis } from './cartesianAxis';
export declare class TimeAxisParentLevel extends BaseProperties {
    enabled: boolean;
    readonly label: AxisLabel;
    readonly tick: AxisTick;
}
export declare class TimeAxis extends CartesianAxis<TimeScale, number | Date> {
    static readonly className = "TimeAxis";
    static readonly type: "time";
    readonly parentLevel: TimeAxisParentLevel;
    min?: Date | number;
    max?: Date | number;
    groupPaddingInner: number;
    paddingInner?: number;
    paddingOuter?: number;
    constructor(moduleCtx: ModuleContext);
    get primaryLabel(): AxisLabel | undefined;
    get primaryTick(): AxisTick | undefined;
    normaliseDataDomain(d: Date[]): {
        domain: Date[];
        clipped: boolean;
    };
}
export declare function normaliseTimeDataDomain(d: Date[], min: Date | number | undefined, max: Date | number | undefined): {
    domain: Date[];
    clipped: boolean;
};
