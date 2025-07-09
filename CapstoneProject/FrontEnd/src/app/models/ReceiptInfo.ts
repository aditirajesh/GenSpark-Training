export interface ReceiptInfo {
  id: string;
  receiptName: string;
  category: string;
  createdAt: Date;
  fileSizeBytes?: number;
  contentType?: string;
  fileData?: Uint8Array;
}