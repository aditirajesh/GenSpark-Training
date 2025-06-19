export declare const BBoxValues: {
    containsPoint: typeof containsPoint;
    equals: typeof equals;
    isEmpty: typeof isEmpty;
    normalize: typeof normalize;
};
export interface BBoxValues {
    x: number;
    y: number;
    width: number;
    height: number;
}
export interface BBoxContainsTester {
    containsPoint(x: number, y: number): boolean;
}
export interface BBoxProvider<T = BBoxValues> {
    id: string;
    toCanvasBBox(): T;
    fromCanvasPoint(x: number, y: number): {
        x: number;
        y: number;
    };
    visible?: boolean;
}
declare function containsPoint(bbox: BBoxValues, x: number, y: number): boolean;
declare function equals(lhs: BBoxValues, rhs: BBoxValues): boolean;
declare function isEmpty(bbox: BBoxValues | undefined): boolean;
declare function normalize(bbox: Partial<BBoxValues>): Partial<BBoxValues>;
export {};
