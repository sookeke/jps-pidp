import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DigitalEvidencePage } from './digital-evidence.page';
import { DigitalEvidenceResolver } from './digital-evidence.resolver';

const routes: Routes = [
  {
    path: '',
    component: DigitalEvidencePage,
    resolve: {
      digitalEvidenceStatusCode: DigitalEvidenceResolver,
    },
    data: {
      title: 'JPS Provider Identity Portal',
      routes: {
        root: '../../',
      },
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DigitalEvidenceRoutingModule {}
