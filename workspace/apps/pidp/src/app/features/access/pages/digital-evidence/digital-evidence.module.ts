import { NgModule } from '@angular/core';

import { SharedModule } from '@app/shared/shared.module';

import { DigitalEvidenceRoutingModule } from './digital-evidence-routing.module';
import { DigitalEvidencePage } from './digital-evidence.page';

@NgModule({
  declarations: [DigitalEvidencePage],
  imports: [DigitalEvidenceRoutingModule, SharedModule],
})
export class DigitalEvidenceModule {}
