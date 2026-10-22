export type Ativo = 'PETR4' | 'VALE3' | 'VIIA4';
export type Lado = 'C' | 'V';

export const ATIVOS: readonly Ativo[] = ['PETR4', 'VALE3', 'VIIA4'];

export interface Ordem {
  ativo: Ativo;
  lado: Lado;
  quantidade: number;
  preco: number;
}

export interface OrdemResposta {
  sucesso: boolean;
  exposicao_atual: number;
  msg_erro: string | null;
}
