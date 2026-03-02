export interface Ban {
  id: number;
  ipAddressHash: string;
  boardId?: number;
  boardShortName?: string;
  reason: string;
  isActive: boolean;
  isExpired: boolean;
  isCurrentlyActive: boolean;
  bannedAt: string;
  expiresAt?: string;
}
