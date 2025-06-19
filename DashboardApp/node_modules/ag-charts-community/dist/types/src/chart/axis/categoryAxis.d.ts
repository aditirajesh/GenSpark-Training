import type { ModuleContext } from '../../module/moduleContext';
import { CategoryScale } from '../../scale/categoryScale';
import type { OrdinalTimeScale } from '../../scale/ordinalTimeScale';
import type { UnitTimeScale } from '../../scale/unitTimeScale';
import { CartesianAxis } from './cartesianAxis';
export declare class CategoryAxis<S extends CategoryScale<string | object> | UnitTimeScale | OrdinalTimeScale = CategoryScale<string | object>> extends CartesianAxis<S> {
    static is(this: void, value: unknown): value is CategoryAxis<any>;
    static readonly className: string;
    static readonly type: 'category' | 'grouped-category' | 'unit-time' | 'ordinal-time';
    constructor(moduleCtx: ModuleContext, scale?: S);
    groupPaddingInner: number;
    paddingInner?: number;
    paddingOuter?: number;
    normaliseDataDomain(domain: Array<string | object>): {
        domain: (string | object)[];
        clipped: boolean;
    };
    protected updateScale(): void;
    private reduceBandScalePadding;
}
