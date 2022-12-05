import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BcpsAuthService {
  private _partyId!: number;
  private _participantId!: number;
  public set partyId(partyId: number) {
    if (!this._partyId) {
      this._partyId = partyId;
    }
  }

  public get partyId(): number {
    return this._partyId;
  }

  public set participantId(participantId: number) {
    if (!this._participantId) {
      this._participantId = participantId;
    }
  }
  public get participantId(): number {
    return this._participantId;
  }
}
