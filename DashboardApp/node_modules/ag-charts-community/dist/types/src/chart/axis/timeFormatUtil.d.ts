import type { TimeIntervalUnit } from 'ag-charts-types';
import type { TimeInterval } from '../../util/time';
export declare function deriveTimeSpecifier(format: string | Record<string, string> | undefined, timeInterval: TimeInterval | TimeIntervalUnit): string | undefined;
