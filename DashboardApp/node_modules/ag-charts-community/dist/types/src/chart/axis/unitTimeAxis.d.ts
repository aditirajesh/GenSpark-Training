import type { TimeIntervalUnit } from 'ag-charts-types';
import type { ModuleContext } from '../../module/moduleContext';
import { UnitTimeScale } from '../../scale/unitTimeScale';
import type { TimeInterval } from '../../util/time';
import type { AxisLabel } from './axisLabel';
import type { AxisTick } from './axisTick';
import { CategoryAxis } from './categoryAxis';
import { TimeAxisParentLevel } from './timeAxis';
export declare class UnitTimeAxis extends CategoryAxis<UnitTimeScale> {
    static readonly className: "UnitTimeAxis";
    static readonly type: "unit-time";
    readonly parentLevel: TimeAxisParentLevel;
    min?: Date | number;
    max?: Date | number;
    unit: TimeInterval | TimeIntervalUnit | undefined;
    get primaryLabel(): AxisLabel | undefined;
    get primaryTick(): AxisTick | undefined;
    constructor(moduleCtx: ModuleContext);
    protected updateScale(): void;
    normaliseDataDomain(domain: Date[]): {
        domain: Date[];
        clipped: boolean;
    };
}
