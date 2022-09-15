import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UsertypeService {
  private _partyId!: number;
  public set partyId(partyId: number) {
    if (!this._partyId) {
      this._partyId = partyId;
    }
  }
  public get partyId(): number{
    return this._partyId;
  }
}
