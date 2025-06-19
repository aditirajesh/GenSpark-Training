import type { TimeIntervalUnit } from 'ag-charts-types';
import { type TimeInterval } from '../util/time';
import { BandScale } from './bandScale';
import type { ScaleFormatParams } from './scale';
export declare abstract class DiscreteTimeScale extends BandScale<Date, TimeInterval | TimeIntervalUnit | number> {
    static is(value: unknown): value is DiscreteTimeScale;
    toDomain(value: number): Date;
    convert(d: Date, options?: {
        interpolate?: boolean;
    }): number;
    invert(position: number, nearest?: boolean): Date | undefined;
    /**
     * Returns a time format function suitable for displaying tick values.
     * @param specifier If the specifier string is provided, this method is equivalent to
     * the {@link TimeLocaleObject.format} method.
     * If no specifier is provided, this method returns the default time format function.
     */
    tickFormatter({ domain, ticks, specifier }: ScaleFormatParams<Date>, formatOffset?: number): (date: Date) => string;
}
