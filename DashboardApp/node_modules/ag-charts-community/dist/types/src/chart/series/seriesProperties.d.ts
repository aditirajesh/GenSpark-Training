import type { RequiredInternalAgGradientColor, RequiredInternalAgImageFill, RequiredInternalAgPatternColor } from 'ag-charts-core';
import type { AgColorRepeat, AgGradientColorBounds, AgGradientColorStop, AgGradientType, AgImageFillFit, AgPatternName, CssColor, InteractionRange, Opacity, PixelSize } from 'ag-charts-types';
import { BaseProperties } from '../../util/properties';
import type { SeriesTooltip } from './seriesTooltip';
export declare class SeriesItemHighlightStyle extends BaseProperties {
    fill?: string;
    fillOpacity?: number;
    stroke?: string;
    strokeWidth?: number;
    strokeOpacity?: number;
    lineDash?: number[];
    lineDashOffset?: number;
}
declare class SeriesHighlightStyle extends BaseProperties {
    strokeWidth?: number;
    dimOpacity?: number;
    enabled?: boolean;
}
declare class TextHighlightStyle extends BaseProperties {
    color?: string;
}
export declare class HighlightProperties extends BaseProperties {
    enabled: boolean;
}
export declare class FillGradientDefaults extends BaseProperties<RequiredInternalAgGradientColor> implements RequiredInternalAgGradientColor {
    type: 'gradient';
    colorStops: AgGradientColorStop[];
    bounds: AgGradientColorBounds;
    gradient: AgGradientType;
    rotation: number;
    reverse: boolean;
}
export declare class FillPatternDefaults extends BaseProperties<RequiredInternalAgPatternColor> implements RequiredInternalAgPatternColor {
    type: 'pattern';
    colorStops: AgGradientColorStop[];
    bounds: AgGradientColorBounds;
    gradient: AgGradientType;
    rotation: number;
    scale: number;
    reverse: boolean;
    path?: string;
    pattern: AgPatternName;
    width: number;
    height: number;
    padding: number;
    fill: CssColor;
    fillOpacity: Opacity;
    backgroundFill: CssColor;
    backgroundFillOpacity: Opacity;
    stroke: CssColor;
    strokeOpacity: number;
    strokeWidth: PixelSize;
}
export declare class FillImageDefaults extends BaseProperties<RequiredInternalAgImageFill> implements RequiredInternalAgImageFill {
    type: 'image';
    url: string;
    rotation: number;
    scale: number;
    backgroundFill: CssColor;
    backgroundFillOpacity: Opacity;
    repeat: AgColorRepeat;
    fit: AgImageFillFit;
}
export declare class HighlightStyle extends BaseProperties {
    readonly item: SeriesItemHighlightStyle;
    readonly series: SeriesHighlightStyle;
    readonly text: TextHighlightStyle;
}
export declare abstract class SeriesProperties<T extends object> extends BaseProperties<T> {
    id?: string;
    protected readonly visible: boolean;
    focusPriority?: number;
    showInLegend: boolean;
    cursor: string;
    nodeClickRange: InteractionRange;
    readonly highlight: HighlightProperties;
    readonly highlightStyle: HighlightStyle;
    abstract tooltip: SeriesTooltip<never>;
    context?: unknown;
    handleUnknownProperties(unknownKeys: Set<string>, properties: T): void;
}
export {};
