import type { RequiredInternalAgImageFill, RequiredInternalAgPatternColor } from 'ag-charts-core';
import type { AgCartesianChartOptions, WithThemeParams } from 'ag-charts-types';
type CartesianAxis = Exclude<AgCartesianChartOptions['axes'], undefined>[0];
export declare const DIRECTION_SWAP_AXES: WithThemeParams<[CartesianAxis, CartesianAxis]>;
export declare const SAFE_FILL_OPERATION: any;
export declare const SAFE_FILLS_OPERATION: any;
export declare const SAFE_STROKE_FILL_OPERATION: any;
export declare const SAFE_RANGE2_OPERATION: any;
export declare const FILL_PATTERN_DEFAULTS: WithThemeParams<RequiredInternalAgPatternColor>;
export declare const FILL_IMAGE_DEFAULTS: WithThemeParams<RequiredInternalAgImageFill>;
export declare function getSequentialColors(colors: {
    [key: string]: string;
}): Record<string | number, string[]>;
export {};
