import type { AgIconName } from 'ag-charts-types';
import { BBoxValues } from './bboxinterface';
export declare function setElementBBox(element: HTMLElement | undefined, bbox: Partial<BBoxValues>): void;
export declare function getElementBBox(element: HTMLElement): BBoxValues;
export declare function focusCursorAtEnd(element: HTMLElement): void;
export declare function isInputPending(): false | void;
export declare function getIconClassNames(icon: AgIconName): string;
