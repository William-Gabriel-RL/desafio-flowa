import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, of } from 'rxjs';
import { Ordem, OrdemResposta } from './ordem.model';

@Injectable({ providedIn: 'root' })
export class OrdemService {
  private readonly http = inject(HttpClient);

  enviar(ordem: Ordem): Observable<OrdemResposta> {
    return this.http.post<OrdemResposta>('/ordens', ordem).pipe(
      catchError((erro: HttpErrorResponse) =>
        of(
          isOrdemResposta(erro.error)
            ? erro.error
            : { sucesso: false, exposicao_atual: null, msg_erro: 'Não foi possível comunicar com o OrderAccumulator.' },
        ),
      ),
    );
  }
}

function isOrdemResposta(valor: unknown): valor is OrdemResposta {
  return typeof valor === 'object' && valor !== null && 'sucesso' in valor;
}
