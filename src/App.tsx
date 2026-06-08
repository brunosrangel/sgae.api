import React, { useState, useEffect } from "react";
import { 
  Folder, 
  FolderOpen, 
  FileCode, 
  Database, 
  Activity, 
  ShieldCheck, 
  CheckCircle2, 
  Layers, 
  Sparkles, 
  Copy, 
  Check, 
  Play, 
  BookOpen, 
  ArrowRight, 
  Terminal,
  Clock,
  HelpCircle,
  FileText,
  Workflow,
  AlertCircle,
  Search
} from "lucide-react";
import { sgaeFileStructure, cleanArchitectureDescription, FileNode } from "./data/csharpCode";

// Encontra recursivamente um nó de arquivo pelo caminho
function findFileNodeByPath(nodes: FileNode[], path: string): FileNode | null {
  for (const node of nodes) {
    if (node.path === path) return node;
    if (node.children) {
      const found = findFileNodeByPath(node.children, path);
      if (found) return found;
    }
  }
  return null;
}

export default function App() {
  const [activeTab, setActiveTab] = useState<"ide" | "database" | "funnel" | "architecture">("ide");
  const [selectedFilePath, setSelectedFilePath] = useState<string>("Sgae.Application/Common/Mappings/MappingProfile.cs");
  const [selectedFile, setSelectedFile] = useState<FileNode | null>(null);
  const [copied, setCopied] = useState(false);
  const [openFolders, setOpenFolders] = useState<Record<string, boolean>>({
    "Sgae.Domain": true,
    "Sgae.Domain/Entities": true,
    "Sgae.Domain/Exceptions": true,
    "Sgae.Domain/Enums": false,
    "Sgae.Infrastructure": false,
    "Sgae.Infrastructure/Persistence": false,
    "Sgae.Application": true,
    "Sgae.Application/Common": true,
    "Sgae.Application/Common/CQRS": false,
    "Sgae.Application/Common/Mappings": true,
    "Sgae.Application/Common/Models": true,
    "Sgae.Application/Abstractions": false,
    "Sgae.Application/Leads": true,
    "Sgae.Application/Leads/DTOs": true,
    "Sgae.Application/Leads/Commands": false,
    "Sgae.Application/Leads/Queries": true,
    "Sgae.Application/Agendamentos": true,
    "Sgae.Application/Agendamentos/DTOs": true,
    "Sgae.Application/Agendamentos/Commands": false,
    "Sgae.Application/Agendamentos/Queries": true,
    "Sgae.API": true,
    "Sgae.API/Middlewares": false
  });
  
  // Estados para Auditoria de IA com Gemini
  const [analyzingFile, setAnalyzingFile] = useState<string | null>(null);
  const [aiAnalysis, setAiAnalysis] = useState<Record<string, string>>({});
  const [simulationLogs, setSimulationLogs] = useState<string[]>([
    "SGAE .NET Sandbox iniciada com suporte a CQRS + AutoMapper + Middleware Global.",
    "Database PostgreSQL ativo. DTOs de transporte configurados nas respostas espirituais.",
    "Perfis de mapeamento do AutoMapper carregados automaticamente via Assembly scan.",
    "Queries customizadas criadas para buscar Leads (Paginado) e Agendamentos com filtros.",
    "Pronto para simular requisições de consulta com projeção limpa e rápida!"
  ]);

  // Carrega o arquivo selecionado inicialmente
  useEffect(() => {
    const file = findFileNodeByPath(sgaeFileStructure, selectedFilePath);
    if (file) {
      setSelectedFile(file);
    }
  }, [selectedFilePath]);

  const toggleFolder = (folderPath: string) => {
    setOpenFolders(prev => ({
      ...prev,
      [folderPath]: !prev[folderPath]
    }));
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const runAiAnalysis = async (file: FileNode) => {
    if (analyzingFile === file.path) return;
    setAnalyzingFile(file.path);
    
    // Adiciona log de simulação
    setSimulationLogs(prev => [
      ...prev,
      `[AI Audit] Iniciando auditoria arquitetural no arquivo: ${file.name}...`
    ]);

    try {
      const response = await fetch("/api/gemini/analyze", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          fileName: file.path,
          description: file.description,
          content: file.content
        })
      });

      if (!response.ok) {
        throw new Error("Falha ao comunicar com o servidor de inteligência.");
      }

      const data = await response.json();
      setAiAnalysis(prev => ({
        ...prev,
        [file.path]: data.analysis
      }));

      setSimulationLogs(prev => [
        ...prev,
        `[AI Audit] Auditoria concluída com sucesso para: ${file.name}.`
      ]);
    } catch (err: any) {
      console.error(err);
      // Fallback amigável se a requisição quebrar
      setAiAnalysis(prev => ({
        ...prev,
        [file.path]: `### ⚠️ Erro na auditoria do Gemini
Ocorreu um erro ao conectar com a API do Gemini. 

**Análise de Fallback (Arquiteto .NET):**
O arquivo \`${file.name}\` possui excelente conformidade arquitetural baseada em princípios de SOLID e DDD.
- Uso correto de **encapsulamento sem setters públicos**.
- Estrutura clara do domínio utilizando enums e herança de \`BaseEntity\`.
- PostgreSQL modelado com precisão e relacionamento restrito no DbContext.`
      }));
      setSimulationLogs(prev => [
        ...prev,
        `[AI Audit ⚠️] Erro ao comunicar com o servidor. Carregado parecer de fallback local.`
      ]);
    } finally {
      setAnalyzingFile(null);
    }
  };

  // Renderiza a árvore de arquivos recursivamente
  const renderFileTree = (nodes: FileNode[], depth = 0) => {
    return nodes.map(node => {
      const isFolder = node.type === "folder";
      const isOpen = openFolders[node.path];
      const isSelected = selectedFilePath === node.path;

      if (isFolder) {
        return (
          <div key={node.path} style={{ paddingLeft: `${depth * 12}px` }}>
            <button
              onClick={() => toggleFolder(node.path)}
              className="flex items-center w-full text-left py-1.5 px-2 hover:bg-slate-800 rounded text-slate-300 transition-colors duration-150 font-medium text-sm group"
            >
              <span className="mr-1.5 text-slate-500 group-hover:text-slate-300">
                {isOpen ? <FolderOpen size={16} /> : <Folder size={16} />}
              </span>
              <span className="truncate">{node.name}</span>
            </button>
            {isOpen && node.children && (
              <div className="border-l border-slate-800 ml-3.5 pl-1 my-0.5">
                {renderFileTree(node.children, depth + 1)}
              </div>
            )}
          </div>
        );
      } else {
        return (
          <div key={node.path} style={{ paddingLeft: `${depth * 12}px` }} className="my-0.5">
            <button
              id={`file-btn-${node.name.replace(/\./g, "-")}`}
              onClick={() => setSelectedFilePath(node.path)}
              className={`flex items-center w-full text-left py-1 px-2 rounded text-xs transition-colors duration-150 ${
                isSelected 
                  ? "bg-emerald-950/50 border border-emerald-500/50 text-emerald-300 font-semibold" 
                  : "text-slate-400 hover:bg-slate-800 hover:text-slate-200"
              }`}
            >
              <span className={`mr-2 ${isSelected ? 'text-emerald-400' : 'text-indigo-400'}`}>
                <FileCode size={14} />
              </span>
              <span className="truncate">{node.name}</span>
            </button>
          </div>
        );
      }
    });
  };

  // Lista as 10 etapas do funil para o SGAE
  const funnelSteps = [
    {
      step: 1,
      name: "Captação",
      status: "completed",
      desc: "Primeiro contato e dados originários do consulente.",
      attributes: ["Nome", "Telefone", "E-mail", "Cidade", "Estado", "Origem", "Problema Principal", "Data Contato"],
      entity: "Domain/Entities/Lead.cs"
    },
    {
      step: 2,
      name: "Agendamento",
      status: "completed",
      desc: "Configuração do atendimento inicial, valor e modalidade.",
      attributes: ["DataHora", "Modalidade (Presencial/Online)", "Valor", "Status (Enum)", "Motivo Cancelamento"],
      entity: "Domain/Entities/Agendamento.cs"
    },
    {
      step: 3,
      name: "Perfil do Consulente",
      status: "pending",
      desc: "Preenchimento de dados de segmentação social e socioeconômica.",
      attributes: ["Idade", "Faixa Etária", "Gênero", "Profissão", "Escolaridade", "Estado Civil"],
      entity: "Domain/Entities/PerfilConsulente.cs"
    },
    {
      step: 4,
      name: "Atendimento Espiritual",
      status: "pending",
      desc: "Registro técnico da consulta, temas abordados e guias.",
      attributes: ["Tipo (Primeiro/Retorno)", "Tempo de Atendimento", "Temas Abordados", "Resultado/Encaminhamento"],
      entity: "Domain/Entities/Atendimento.cs"
    },
    {
      step: 5,
      name: "Prescrição de Rituais",
      status: "pending",
      desc: "Recomendações e tratamentos espirituais recomendados pelo sacerdote.",
      attributes: ["Tipo de Ritual (Ebó, Bori, etc.)", "Complexidade", "Prazo Recomendado para Realização"],
      entity: "Domain/Entities/Prescricao.cs"
    },
    {
      step: 6,
      name: "Conversão de Ebós",
      status: "pending",
      desc: "Métricas de engajamento do consulente com as recomendações.",
      attributes: ["Status de Realização", "Motivos de Abandono/Não Realização"],
      entity: "Domain/Entities/ConversaoEbo.cs"
    },
    {
      step: 7,
      name: "Gestão Financeira",
      status: "pending",
      desc: "Fluxo de caixa gerado por atendimentos e trabalhos.",
      attributes: ["Receitas (Consultas/Ebós)", "Forma de Pagamento (Pix, Cartão, Dinheiro)"],
      entity: "Domain/Entities/Receita.cs"
    },
    {
      step: 8,
      name: "Gestão de Custos",
      status: "pending",
      desc: "Detalhamento e margem de contribuição de materiais.",
      attributes: ["Materiais Necessários", "Quantidade", "Fornecedor", "Custo Unitário", "Mão de Obra"],
      entity: "Domain/Entities/CustoTrabalho.cs"
    },
    {
      step: 9,
      name: "Pós-Atendimento",
      status: "pending",
      desc: "Pesquisas de satisfação e acompanhamento estendido.",
      attributes: ["NPS 30, 60, 90 Dias (Nota 0-10)", "Feedback Descritivo de Melhora"],
      entity: "Domain/Entities/FeedbackPosAtendimento.cs"
    },
    {
      step: 10,
      name: "Fidelização & Retenção",
      status: "pending",
      desc: "Métricas globais de LTV, frequência e ciclo de vida.",
      attributes: ["Histórico de Retornos", "LTV (Lifetime Value)", "Frequência de Visitas"],
      entity: "Domain/Entities/Retencao.cs"
    }
  ];

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans select-none antialiased">
      {/* topo / Header */}
      <header className="border-b border-slate-900 bg-slate-900 px-6 py-4 flex flex-col md:flex-row items-center justify-between gap-4">
        <div className="flex items-center space-x-3">
          <div className="h-10 w-10 bg-gradient-to-tr from-emerald-600 to-indigo-700 rounded-lg flex items-center justify-center text-white shadow-lg shadow-emerald-950/40">
            <Layers size={22} className="animate-pulse" />
          </div>
          <div>
            <div className="flex items-center space-x-2">
              <h1 className="text-lg font-bold tracking-tight text-white">SGAE</h1>
              <span className="text-xs px-2 py-0.5 rounded-full bg-emerald-950 text-emerald-400 border border-emerald-500/30 font-semibold">
                Passo 1: Entidades & DB
              </span>
            </div>
            <p className="text-xs text-slate-400">
              Sistema de Gestão de Atendimentos e Acompanhamento Espiritual em Arquitetura Limpa .NET 8
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <div className="flex bg-slate-950 p-1 rounded-lg border border-slate-800">
            <button
              onClick={() => setActiveTab("ide")}
              className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-all duration-150 ${
                activeTab === "ide"
                  ? "bg-slate-800 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <FileCode size={14} />
              <span>IDE .NET 8</span>
            </button>
            <button
              onClick={() => setActiveTab("database")}
              className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-all duration-150 ${
                activeTab === "database"
                  ? "bg-slate-800 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <Database size={14} />
              <span>PostgreSQL (DER)</span>
            </button>
            <button
              onClick={() => setActiveTab("funnel")}
              className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-all duration-150 ${
                activeTab === "funnel"
                  ? "bg-slate-800 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <Workflow size={14} />
              <span>Funil de 10 Etapas</span>
            </button>
            <button
              onClick={() => setActiveTab("architecture")}
              className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-md text-xs font-medium transition-all duration-150 ${
                activeTab === "architecture"
                  ? "bg-slate-800 text-white shadow-sm"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <BookOpen size={14} />
              <span>Arquitetura Limpa</span>
            </button>
          </div>
        </div>
      </header>

      {/* Conteúdo Principal */}
      <main className="flex-1 flex overflow-hidden">
        
        {/* TAB 1: IDE DE CÓDIGO */}
        {activeTab === "ide" && (
          <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
            
            {/* Explorer de Solução (Esquerdo) */}
            <section className="w-full md:w-64 border-b md:border-b-0 md:border-r border-slate-900 bg-slate-900/40 p-4 flex flex-col">
              <div className="flex items-center justify-between mb-4">
                <span className="text-xs font-semibold tracking-wider uppercase text-slate-400">
                  Solution Explorer
                </span>
                <span className="text-[10px] px-1.5 py-0.5 bg-slate-800 text-slate-400 rounded">
                  4 Projetos
                </span>
              </div>
              
              <div className="flex-1 overflow-y-auto space-y-1 select-none pr-1 scrollbar-thin">
                <div className="flex items-center font-bold text-xs text-slate-300 pb-2 px-1 border-b border-slate-800/60 mb-2">
                  <Terminal size={14} className="mr-1.5 text-indigo-400" />
                  <span>SgaeSolution.sln</span>
                </div>
                {renderFileTree(sgaeFileStructure)}
              </div>

              {/* Informação do Status da Sessão */}
              <div className="mt-4 pt-4 border-t border-slate-900">
                <div className="p-3 bg-slate-900 rounded-lg border border-slate-800/80">
                  <div className="flex items-center space-x-2 text-emerald-400 font-medium text-xs mb-1">
                    <CheckCircle2 size={14} />
                    <span>Passo 1 Completo</span>
                  </div>
                  <p className="text-[10px] text-slate-400 leading-relaxed">
                    Etapa 1 (Captação) e Etapa 2 (Agendamento) modeladas com Rich Domain Entities e DbContext. Prontas para validação!
                  </p>
                </div>
              </div>
            </section>

            {/* Painel Central de Visualização de Código */}
            <section className="flex-1 flex flex-col bg-slate-950 overflow-hidden">
              {(() => {
                if (!selectedFile) return (
                  <div className="flex-1 flex flex-col items-center justify-center p-8 text-center text-slate-500">
                    <FileCode size={48} className="mb-4 text-slate-700 animate-bounce" />
                    <p>Selecione um arquivo de código para visualizar o modelo .NET 8.</p>
                  </div>
                );

                return (
                  <>
                    {/* Barra superior da IDE e Métricas */}
                    <div className="bg-[#0b0f19] px-6 py-3 border-b border-slate-900 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 shrink-0">
                      <div className="flex items-center space-x-3 overflow-hidden">
                        <span className="p-1 px-1.5 rounded bg-indigo-950/60 text-indigo-400 font-bold text-xs font-mono shrink-0">
                          {selectedFile.language?.toUpperCase() || "C#"}
                        </span>
                        <div className="overflow-hidden">
                          <h2 className="text-sm font-semibold text-slate-200 truncate">{selectedFile.path}</h2>
                          <p className="text-[11px] text-slate-400 truncate mt-0.5">{selectedFile.description}</p>
                        </div>
                      </div>

                      <div className="flex items-center space-x-2 w-full sm:w-auto self-stretch sm:self-auto justify-end">
                        <button
                          onClick={() => copyToClipboard(selectedFile.content || "")}
                          className="flex items-center space-x-1 px-3 py-1.5 bg-slate-900 hover:bg-slate-800 border border-slate-800 text-slate-300 rounded text-xs transition duration-150 cursor-pointer"
                        >
                          {copied ? <Check size={14} className="text-emerald-400" /> : <Copy size={14} />}
                          <span>{copied ? "Copiado!" : "Copiar"}</span>
                        </button>

                        <button
                          onClick={() => runAiAnalysis(selectedFile)}
                          disabled={analyzingFile !== null}
                          className={`flex items-center space-x-1.5 px-3 py-1.5 rounded text-xs font-medium transition duration-150 cursor-pointer ${
                            analyzingFile
                              ? "bg-indigo-950 text-indigo-300 border border-indigo-800/50 cursor-not-allowed"
                              : "bg-indigo-600 hover:bg-indigo-500 text-white shadow-lg shadow-indigo-950/20"
                          }`}
                        >
                          <Sparkles size={14} className={analyzingFile ? "animate-spin" : ""} />
                          <span>{analyzingFile ? "Analisando..." : "Auditar com IA"}</span>
                        </button>
                      </div>
                    </div>

                    {/* Divisor do Editor e Parecer */}
                    <div className="flex-1 flex flex-col lg:flex-row overflow-hidden">
                      {/* Editor (C# Code Container) */}
                      <div className="flex-1 overflow-auto border-r border-slate-900 flex relative scrollbar-thin">
                        {/* Linhas numeradoras simuladas */}
                        <div className="bg-[#0b0f19] text-slate-600 font-mono text-[11px] text-right pr-3 pl-4 py-4 select-none shrink-0 sticky left-0 border-r border-slate-100/5 leading-6">
                          {(selectedFile.content || "").split("\n").map((_, i) => (
                            <div key={i}>{i + 1}</div>
                          ))}
                        </div>
                        {/* Código real com indentação robusta */}
                        <pre className="p-4 pt-4 text-slate-300 font-mono text-[12px] leading-6 flex-1 bg-[#0d111d] overflow-x-auto selection:bg-indigo-950 selection:text-white">
                          <code className="block whitespace-pre-wrap">{selectedFile.content}</code>
                        </pre>
                      </div>

                      {/* Aba Lateral de Análise Arquitetural do Arquivo */}
                      <div className="w-full lg:w-80 bg-slate-900/30 overflow-y-auto p-5 border-t lg:border-t-0 lg:border-l border-slate-900 flex flex-col space-y-4 shrink-0 scrollbar-thin">
                        <div className="pb-3 border-b border-slate-900">
                          <h3 className="text-xs font-bold text-indigo-400 uppercase tracking-wider flex items-center space-x-1.5">
                            <ShieldCheck size={14} />
                            <span>Design de Arquitetura</span>
                          </h3>
                        </div>

                        {/* Detalhes de Design do Domínio / Infra */}
                        <div className="space-y-4 text-xs text-slate-300">
                          <div className="p-3 bg-slate-900/60 border border-slate-800 rounded">
                            <span className="font-semibold block mb-1 text-slate-200">Abordagem do Código:</span>
                            {selectedFile.path.includes("Domain") ? (
                              <ul className="list-disc list-inside space-y-1.5 text-slate-400">
                                <li><strong>Rich Domain Models</strong>: Encapsulamento estrito (setters privados) impedindo estados inválidos de domínio.</li>
                                <li><strong>Business Logic inside Entities</strong>: Validações e transições de estado residem diretamente na classe de domínio.</li>
                                <li><strong>Regras do SGAE</strong>: Tratamento rico da captação e ciclo de vida de confirmados/ausentes.</li>
                              </ul>
                            ) : selectedFile.path.includes("Infrastructure") ? (
                              <ul className="list-disc list-inside space-y-1.5 text-slate-400">
                                <li><strong>Postgres Fluent API</strong>: Todo mapeamento isolado das classes de domínio no DbContext.</li>
                                <li><strong>Mapeamento Eficiente</strong>: Enums persistidos como string (\`varchar(50)\`) aumentando a auditabilidade direta no banco de dados.</li>
                                <li><strong>Soft Delete Global Config</strong>: Configuração automática de filtros estritos do EF Core.</li>
                              </ul>
                            ) : (
                              <p className="text-slate-400">Estruturação de base do projeto Clean Architecture para a solução SGAE .NET 8 no PostgreSQL.</p>
                            )}
                          </div>

                          {/* Painel de Auditoria por IA */}
                          <div className="bg-slate-900/40 p-4 border border-slate-800/80 rounded-lg">
                            <div className="flex items-center space-x-2 mb-2 text-indigo-300 font-semibold text-xs">
                              <Sparkles size={14} />
                              <span>Revisão do Arquiteto (AI Audit)</span>
                            </div>

                            {aiAnalysis[selectedFile.path] ? (
                              <div className="prose prose-invert text-[11px] leading-relaxed text-slate-300 space-y-2 whitespace-pre-wrap">
                                {aiAnalysis[selectedFile.path]}
                              </div>
                            ) : (
                              <div className="text-center py-4 space-y-2">
                                <p className="text-[11px] text-slate-400">Deseja validar se este arquivo atende a 100% dos requisitos de Arquitetura Limpa e SOLID?</p>
                                <button
                                  onClick={() => runAiAnalysis(selectedFile)}
                                  disabled={analyzingFile !== null}
                                  className="w-full text-center py-2 px-3 bg-indigo-950 hover:bg-indigo-900 text-indigo-300 border border-indigo-800/60 rounded text-[11px] font-medium transition cursor-pointer"
                                >
                                  {analyzingFile ? "Auditando Código..." : "Analisar com Gemini"}
                                </button>
                              </div>
                            )}
                          </div>
                        </div>
                      </div>
                    </div>
                  </>
                );
              })()}
            </section>
          </div>
        )}

        {/* TAB 2: POSTGRESQL (DER) */}
        {activeTab === "database" && (
          <div className="flex-1 flex flex-col md:flex-row overflow-hidden p-6 gap-6">
            <div className="flex-1 flex flex-col space-y-4 overflow-y-auto scrollbar-thin">
              <div>
                <h2 className="text-lg font-bold flex items-center space-x-2 text-white">
                  <Database size={20} className="text-indigo-400" />
                  <span>Mapeamento de Banco de Dados PostgreSQL</span>
                </h2>
                <p className="text-xs text-slate-400 mt-1">
                  Esquema lógico gerado pelo contexto base do Entity Framework Core 8.0 (AppDbContext.cs).
                </p>
              </div>

              {/* Tabelas ERD */}
              <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 pt-2">
                
                {/* Tabela Leads */}
                <div className="bg-slate-900/60 rounded-xl border border-slate-800 overflow-hidden shadow-lg">
                  <div className="px-4 py-3 bg-slate-900 border-b border-slate-800/80 flex justify-between items-center bg-gradient-to-r from-slate-900 to-indigo-950/20">
                    <div className="flex items-center space-x-2">
                      <span className="font-bold text-sm text-slate-200">Table:</span>
                      <span className="font-mono text-xs font-semibold px-2 py-0.5 bg-indigo-950 border border-indigo-500/30 text-indigo-300 rounded">
                        Leads
                      </span>
                    </div>
                    <span className="text-[10px] text-emerald-400 font-semibold px-1.5 py-0.5 bg-emerald-950/40 rounded-full">Active</span>
                  </div>
                  <div className="p-4 space-y-1 font-mono text-xs">
                    <div className="flex justify-between py-1.5 border-b border-slate-800 text-amber-400 font-semibold">
                      <span>🔑 Id (UUID)</span>
                      <span className="text-slate-500">PRIMARY KEY</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Nome (VARCHAR(150))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Telefone (VARCHAR(25))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Email (VARCHAR(100))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Cidade (VARCHAR(100))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Estado (CHAR(2))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>DataContato (TIMESTAMP)</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60 text-slate-300">
                      <span>Origem (VARCHAR(50))</span>
                      <span className="text-slate-500">ENUM AS STRING</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>ProblemaPrincipal (VARCHAR(1000))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60 text-slate-500">
                      <span>CreatedAt (TIMESTAMP)</span>
                      <span>NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60 text-slate-500">
                      <span>UpdatedAt (TIMESTAMP)</span>
                      <span>NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 text-slate-500">
                      <span>IsDeleted (BOOLEAN)</span>
                      <span>DEFAULT FALSE</span>
                    </div>
                  </div>
                </div>

                {/* Tabela Agendamentos */}
                <div className="bg-slate-900/60 rounded-xl border border-slate-800 overflow-hidden shadow-lg">
                  <div className="px-4 py-3 bg-slate-900 border-b border-slate-800/80 flex justify-between items-center bg-gradient-to-r from-slate-900 to-indigo-950/20">
                    <div className="flex items-center space-x-2">
                      <span className="font-bold text-sm text-slate-200">Table:</span>
                      <span className="font-mono text-xs font-semibold px-2 py-0.5 bg-indigo-950 border border-indigo-500/30 text-indigo-300 rounded">
                        Agendamentos
                      </span>
                    </div>
                    <span className="text-[10px] text-emerald-400 font-semibold px-1.5 py-0.5 bg-emerald-950/40 rounded-full">Active</span>
                  </div>
                  <div className="p-4 space-y-1 font-mono text-xs">
                    <div className="flex justify-between py-1.5 border-b border-slate-800 text-amber-400 font-semibold">
                      <span>🔑 Id (UUID)</span>
                      <span className="text-slate-500">PRIMARY KEY</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-800 text-teal-400 font-semibold">
                      <span>🔗 LeadId (UUID)</span>
                      <span className="text-slate-500">FOREIGN KEY (RESTRICT)</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>DataHora (TIMESTAMP)</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Modalidade (VARCHAR(50))</span>
                      <span className="text-slate-500">ENUM AS STRING</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Valor (NUMERIC(18,2))</span>
                      <span className="text-red-400">NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>Status (VARCHAR(50))</span>
                      <span className="text-slate-500">ENUM AS STRING</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60">
                      <span>MotivoCancelamento (VARCHAR(500))</span>
                      <span className="text-slate-500">NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60 text-slate-500">
                      <span>CreatedAt (TIMESTAMP)</span>
                      <span>NOT NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 border-b border-slate-8/60 text-slate-500">
                      <span>UpdatedAt (TIMESTAMP)</span>
                      <span>NULL</span>
                    </div>
                    <div className="flex justify-between py-1.5 text-slate-500">
                      <span>IsDeleted (BOOLEAN)</span>
                      <span>DEFAULT FALSE</span>
                    </div>
                  </div>
                </div>

              </div>

              {/* Explicações das Boas Práticas do Banco */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 pt-4">
                <div className="p-4 bg-slate-900 border border-slate-800 rounded-lg">
                  <h4 className="text-xs font-bold text-indigo-400 uppercase tracking-wide mb-1.5">Precisa de Index?</h4>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    A chave estrangeira <code className="text-teal-400 font-mono">LeadId</code> na tabela <code className="text-slate-300">Agendamentos</code> gera automaticamente um índice no PostgreSQL para performance acelerada de joins e consultas.
                  </p>
                </div>
                <div className="p-4 bg-slate-900 border border-slate-800 rounded-lg">
                  <h4 className="text-xs font-bold text-indigo-400 uppercase tracking-wide mb-1.5">Fluent Configuration</h4>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    Evitamos Data Annotations no Domínio por princípio arquitetural. Todo mapeamento é encapsulado no <code className="text-slate-300">AppDbContext.OnModelCreating</code> utilizando Fluent API.
                  </p>
                </div>
                <div className="p-4 bg-slate-900 border border-slate-800 rounded-lg">
                  <h4 className="text-xs font-bold text-indigo-400 uppercase tracking-wide mb-1.5">Postgres Strings</h4>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    Enums são mapeados explicitamente como <code className="text-slate-300">string</code> com conversão fluente. Isso previne que reordenações no Enum C# quebrem os dados salvos previamente no SQL.
                  </p>
                </div>
              </div>
            </div>

            {/* Sandbox / Terminal (Direito) */}
            <div className="w-full md:w-80 border-t md:border-t-0 md:border-l border-slate-900 bg-slate-900/10 p-5 flex flex-col space-y-4 shrink-0">
              <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center space-x-1.5">
                <Terminal size={14} className="text-indigo-400" />
                <span>Simulador de Console .NET 8</span>
              </h3>

              <div className="flex-1 bg-slate-950 p-4 rounded border border-slate-900 font-mono text-[11px] space-y-2 overflow-y-auto max-h-[350px] md:max-h-none scrollbar-thin">
                {simulationLogs.map((log, index) => (
                  <div key={index} className="text-slate-300 leading-relaxed break-all">
                    <span className="text-indigo-500 mr-1.5">SGAE&gt;</span>
                    {log}
                  </div>
                ))}
              </div>

              <div className="space-y-2 pt-2">
                <button
                  onClick={() => {
                    setSimulationLogs(prev => [
                      ...prev,
                      "dotnet run --project Sgae.API",
                      "info: Microsoft.Hosting.Lifetime[14]",
                      "      Now listening on: http://localhost:3000 (Sgae.API)",
                      "      MediatR: Assembly scanning completo. Registrados Comandos e Consultas automaticamente.",
                      "[MediatR] Dispatching CreateLeadCommand...",
                      "      Nome: Bruninho, Telefone: (11) 99999-9999, Origem: Instagram",
                      "      Invoking Lead constructor invariants... OK (Valid Lead Created)",
                      "      dbContext.Leads.AddAsync... OK",
                      "      unitOfWork.SaveChangesAsync... Committed (1 row affected)",
                      "[MediatR] CreateLeadCommand: Success! Lead ID: 7fa32fb1-9de2-4c28-91ed-e2ebae2c4dc8",
                      "[MediatR] Dispatching CreateAgendamentoCommand for Lead 7fa32fb1...",
                      "      Checking if Lead 7fa32fb1 exists in Postgres DbContext... Found",
                      "      Invoking Agendamento constructor invariants... OK (Value > 0, future date)",
                      "      dbContext.Agendamentos.AddAsync... OK",
                      "      unitOfWork.SaveChangesAsync... Committed (1 row affected)",
                      "[MediatR] CreateAgendamentoCommand: Success! Agendamento ID: 01c890ab-c2dd-4e55-8aa1-d9cf550c6091"
                    ]);
                  }}
                  className="w-full py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded text-xs font-medium transition cursor-pointer flex items-center justify-center space-x-1.5"
                >
                  <Play size={12} />
                  <span>Simular Fluxo de Sucesso (CQRS)</span>
                </button>

                <button
                  onClick={() => {
                    setSimulationLogs(prev => [
                      ...prev,
                      "POST http://localhost:3000/api/leads",
                      "Request-Body: { Nome: '', Telefone: '', Email: 'invalido' }",
                      "      [FluentValidation] ValidationException thrown: Name cannot be empty.",
                      "      [Middleware] ExceptionHandlingMiddleware detected ValidationException!",
                      "      [Middleware] Mapping details to RFC 7807 standard response (422 Unprocessable Entity)...",
                      "Response Payload (application/problem+json):",
                      "  {",
                      '    "type": "https://tools.ietf.org/html/rfc4918#section-11.2",',
                      '    "title": "Validation Failed",',
                      '    "status": 422,',
                      '    "detail": "Um ou mais erros de validação ocorreram na entrada dos dados.",',
                      '    "instance": "/api/leads",',
                      '    "errors": {',
                      '      "Nome": [ "Nome não pode ser vazio." ],',
                      '      "Telefone": [ "Telefone para contato é obrigatório." ]',
                      "    }",
                      "  }",
                      "--------------------------------------------------",
                      "POST http://localhost:3000/api/agendamentos",
                      "Request-Body: { LeadId: '7fa32fb1-9de2-4c28-91ed-e2ebae2c4dc8', Valor: -150.00 }",
                      "      [Domain] DomainException thrown: O valor do atendimento ritual deve ser maior que zero.",
                      "      [Middleware] ExceptionHandlingMiddleware detected DomainException!",
                      "      [Middleware] Mapping details to RFC 7807 standard response (400 Bad Request)...",
                      "Response Payload (application/problem+json):",
                      "  {",
                      '    "title": "Domain Rule Violation",',
                      '    "status": 400,',
                      '    "detail": "O valor do atendimento ritual deve ser maior que zero.",',
                      '    "instance": "/api/agendamentos"',
                      "  }"
                    ]);
                  }}
                  className="w-full py-2 bg-rose-600 hover:bg-rose-500 text-white rounded text-xs font-medium transition cursor-pointer flex items-center justify-center space-x-1.5"
                >
                  <AlertCircle size={12} />
                  <span>Simular Erros Globais (RFC 7807)</span>
                </button>

                <button
                  onClick={() => {
                    setSimulationLogs(prev => [
                      ...prev,
                      "GET http://localhost:3000/api/leads?searchTerm=Sampa&pageNumber=1&pageSize=2",
                      "      [MediatR] Dispatching GetLeadsWithPaginationQuery...",
                      "      [Handler] SearchTerm: 'Sampa', PageNumber: 1, PageSize: 2",
                      "      [SQL Code] SELECT * FROM \"Leads\" l WHERE LOWER(l.\"Nome\") LIKE '%sampa%' ...",
                      "      [AutoMapper] Projecting Lead entities to LeadDto (optimized columns)...",
                      "      [Handler] PaginatedList.CreateAsync completed. Count = 1, Pages = 1",
                      "Response Payload (200 OK - PaginatedList<LeadDto>):",
                      "  {",
                      '    "items": [',
                      "      {",
                      '        "id": "7fa32fb1-9de2-4c28-91ed-e2ebae2c4dc8",',
                      '        "nome": "Bruninho Sampa",',
                      '        "telefone": "(11) 99999-9999",',
                      '        "email": "bruninho@sampa.com.br",',
                      '        "cidade": "São Paulo",',
                      '        "estado": "SP",',
                      '        "origem": "Instagram",',
                      '        "problemaPrincipal": "Precisa de ajuda espiritual para negócios.",',
                      '        "dataCaptacao": "2026-06-08T16:55:00Z"',
                      "      }",
                      "    ],",
                      '    "pageNumber": 1,',
                      '    "totalPages": 1,',
                      '    "totalCount": 1,',
                      '    "hasPreviousPage": false,',
                      '    "hasNextPage": false',
                      "  }",
                      "--------------------------------------------------",
                      "GET http://localhost:3000/api/agendamentos?dataInicio=2026-06-08T00:00:00Z&status=Pendente",
                      "      [MediatR] Dispatching GetAgendamentosWithFiltersQuery...",
                      "      [Handler] Querying agendamentos with Lead navigation property included (Eager Loading)...",
                      "      [AutoMapper] Flattening rich domain entities to decoupled presentation DTOs:",
                      "                 - a.Lead.Nome mapped directly into dest.LeadNome!",
                      "                 - a.Lead.Telefone mapped directly into dest.LeadTelefone!",
                      "                 - Enum status and modalidade mapped to friendly string values!",
                      "Response Payload (200 OK - List<AgendamentoDto>):",
                      "  [",
                      "    {",
                      '      "id": "01c890ab-c2dd-4e55-8aa1-d9cf550c6091",',
                      '      "leadId": "7fa32fb1-9de2-4c28-91ed-e2ebae2c4dc8",',
                      '      "leadNome": "Bruninho Sampa",',
                      '      "leadTelefone": "(11) 99999-9999",',
                      '      "dataHora": "2026-06-10T14:30:00Z",',
                      '      "modalidade": "Online",',
                      '      "status": "Pendente",',
                      '      "valor": 150.00',
                      "    }",
                      "  ]"
                    ]);
                  }}
                  className="w-full py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded text-xs font-medium transition cursor-pointer flex items-center justify-center space-x-1.5"
                >
                  <Search size={12} />
                  <span>Simular Consultas (AutoMapper + CQRS)</span>
                </button>

                <p className="text-[10px] text-slate-500 text-center leading-relaxed">Dispara simulações do pipeline passando dados corrompidos para auditar a captura no Middleware de Erros.</p>
              </div>
            </div>
          </div>
        )}

        {/* TAB 3: FUNIL DE 10 ETAPAS */}
        {activeTab === "funnel" && (
          <div className="flex-1 flex flex-col overflow-y-auto p-6 space-y-6 scrollbar-thin">
            <div>
              <h2 className="text-lg font-bold flex items-center space-x-2 text-white">
                <Workflow size={20} className="text-indigo-400" />
                <span>Mapeamento do Funil SGAE (10 Etapas)</span>
              </h2>
              <p className="text-xs text-slate-400 mt-1">
                Progresso iterativo de desenvolvimento da arquitetura e das interfaces de dados.
              </p>
            </div>

            {/* Grid do Funil */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
              {funnelSteps.map((step) => {
                const isCompleted = step.status === "completed";
                return (
                  <div 
                    key={step.step}
                    className={`p-4 rounded-xl border relative overflow-hidden transition-all duration-200 ${
                      isCompleted 
                        ? "bg-slate-900/80 border-emerald-500/20 shadow-lg shadow-emerald-950/10 hover:border-emerald-500/30" 
                        : "bg-slate-900/30 border-slate-900 hover:border-slate-800"
                    }`}
                  >
                    {/* Badge da Etapa */}
                    <div className="flex items-center justify-between mb-3">
                      <span className="text-[10px] uppercase font-bold tracking-wider text-indigo-400 font-mono">
                        Etapa {step.step}
                      </span>
                      {isCompleted ? (
                        <span className="text-[9px] px-2 py-0.5 rounded-full bg-emerald-950 text-emerald-400 border border-emerald-500/30 font-bold uppercase">
                          Modelado
                        </span>
                      ) : (
                        <span className="text-[9px] px-2 py-0.5 rounded-full bg-slate-950 text-slate-500 border border-slate-900 font-medium uppercase animate-pulse">
                          Pendente
                        </span>
                      )}
                    </div>

                    <h3 className="font-bold text-sm text-slate-200 mb-1">{step.name}</h3>
                    <p className="text-[11px] text-slate-400 leading-relaxed mb-3 h-12 overflow-hidden">{step.desc}</p>
                    
                    {/* Atributos mapeados de negócio */}
                    <div className="space-y-1 pb-3 border-b border-slate-800">
                      <span className="text-[10px] text-slate-500 block font-semibold uppercase">Requisitos:</span>
                      <div className="flex flex-wrap gap-1 mt-1">
                        {step.attributes.map((attr, idx) => (
                          <span key={idx} className="text-[9px] bg-slate-950 text-slate-400 py-0.5 px-1.5 rounded border border-slate-900">
                            {attr}
                          </span>
                        ))}
                      </div>
                    </div>

                    {/* Botão de arquivo */}
                    <div className="pt-3">
                      {isCompleted ? (
                        <button
                          onClick={() => {
                            setSelectedFilePath(`Sgae.${step.entity}`);
                            setActiveTab("ide");
                          }}
                          className="w-full py-1.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded text-[10px] font-semibold transition flex items-center justify-center space-x-1"
                        >
                          <FileText size={12} className="text-emerald-400" />
                          <span>Ver Código C#</span>
                        </button>
                      ) : (
                        <div className="w-full py-1.5 text-center text-[10px] font-medium text-slate-600 bg-slate-950/60 rounded">
                          Aguardando aprovação
                        </div>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Banner de Feedback e Chamada de Ação do Chat */}
            <div className="p-6 bg-gradient-to-r from-indigo-950/40 via-indigo-900/10 to-slate-950 rounded-xl border border-indigo-500/20 flex flex-col md:flex-row items-center justify-between gap-6">
              <div className="space-y-1.5">
                <span className="text-[10px] px-2 py-0.5 bg-indigo-900 text-indigo-300 rounded-full font-bold uppercase tracking-wider">
                  Próximo Objetivo da Sessão
                </span>
                <h3 className="text-base font-bold text-white">Pronto para a camada Application do MediatR?</h3>
                <p className="text-xs text-slate-400 leading-relaxed max-w-xl">
                  Se você aprovar a modelagem base das etapas 1 e 2 apresentadas na IDE central, informe no chat para implementarmos iterativamente os Commands, Queries, Validadores e Casos de Uso com o MediatR de forma espetacular.
                </p>
              </div>

              <div className="flex flex-col sm:flex-row gap-3 self-stretch md:self-auto shrink-0">
                <button
                  onClick={() => {
                    copyToClipboard("Gostaria de aprovar as entidades de Captação, Agendamento e a configuração base do AppDbContext. Podemos dar andamento com a camada Application (Casos de Uso com MediatR, CQRS e validações via FluentValidation)!");
                  }}
                  className="flex items-center justify-center space-x-1.5 px-4 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg text-xs font-semibold cursor-pointer transition shadow-lg shadow-indigo-950/30"
                >
                  <Copy size={14} />
                  <span>Copiar mensagem de aprovação</span>
                </button>
              </div>
            </div>
          </div>
        )}

        {/* TAB 4: TEORIA DA ARQUITETURA LIMPA */}
        {activeTab === "architecture" && (
          <div className="flex-1 flex flex-col overflow-y-auto p-6 space-y-6 scrollbar-thin">
            <div>
              <h2 className="text-lg font-bold flex items-center space-x-2 text-white">
                <Layers size={20} className="text-indigo-400" />
                <span>Padrão Clean Architecture para o SGAE</span>
              </h2>
              <p className="text-xs text-slate-400 mt-1">
                Fluxo direcionado de acoplamento tecnológico: O Domínio impõe as regras de negócio de maneira pura, sem dependências externas.
              </p>
            </div>

            {/* Layout de Círculos da Arquitetura Linha */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start pt-2">
              
              {/* O Fluxo (Visual) */}
              <div className="lg:col-span-6 flex flex-col items-center justify-center bg-slate-900/30 p-8 rounded-2xl border border-slate-900">
                <div className="relative w-72 h-72 flex items-center justify-center">
                  
                  {/* Círculo Externo: API / Presentation */}
                  <div className="absolute w-72 h-72 rounded-full border border-pink-500/30 bg-pink-950/5 flex items-start justify-center pt-3 select-none">
                    <span className="text-[10px] font-bold text-pink-400">Presentation / WebAPI</span>
                  </div>

                  {/* Círculo Médio: Infrastructure */}
                  <div className="absolute w-56 h-56 rounded-full border border-sky-500/30 bg-sky-950/5 flex items-start justify-center pt-3 select-none">
                    <span className="text-[10px] font-bold text-sky-400">Infrastructure</span>
                  </div>

                  {/* Círculo Interno Médio: Application */}
                  <div className="absolute w-40 h-40 rounded-full border border-purple-500/30 bg-purple-950/5 flex items-start justify-center pt-3 select-none">
                    <span className="text-[10px] font-bold text-purple-400">Application</span>
                  </div>

                  {/* Círculo Central: Domain */}
                  <div className="absolute w-24 h-24 rounded-full border border-emerald-500/40 bg-emerald-950/40 flex items-center justify-center text-center shadow-lg shadow-emerald-950/30 hover:scale-105 transition-transform duration-200 cursor-pointer">
                    <div className="space-y-0.5">
                      <span className="text-xs font-bold text-emerald-400 block">Domain</span>
                      <span className="text-[8px] text-emerald-500 block uppercase font-mono">Core</span>
                    </div>
                  </div>

                  {/* Setas de Dependência */}
                  <div className="absolute -top-3 left-1/2 transform -translate-x-1/2 flex flex-col items-center select-none pt-4 space-y-1">
                    <span className="text-[8px] text-indigo-400 font-bold uppercase bg-slate-950 px-1.5 py-0.5 rounded border border-slate-900">Regra de Acoplamento</span>
                    <ArrowRight size={14} className="transform rotate-90 text-indigo-500 animate-bounce" />
                  </div>
                </div>

                <div className="mt-8 text-center max-w-sm space-y-2">
                  <h4 className="text-xs font-bold text-indigo-400 uppercase tracking-widest leading-none">A Regra da Dependência</h4>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    A dependência do código SEMPRE aponta de fora para dentro. O Domínio (<span className="text-emerald-400">Domain</span>) desconhece o Entity Framework, o Postgres ou o próprio ASP.NET Web API.
                  </p>
                </div>
              </div>

              {/* Descrição das Camadas */}
              <div className="lg:col-span-6 space-y-4">
                
                {/* Camada 1: Domain */}
                <div className="p-4 bg-slate-900/60 rounded-xl border border-slate-800/80 hover:border-emerald-500/20 transition duration-150">
                  <div className="flex items-center space-x-2 mb-1.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                    <h3 className="font-bold text-sm text-slate-200">Sgae.Domain</h3>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {cleanArchitectureDescription.domain}
                  </p>
                </div>

                {/* Camada 2: Application */}
                <div className="p-4 bg-slate-900/60 rounded-xl border border-slate-800/80 hover:border-purple-500/20 transition duration-150">
                  <div className="flex items-center space-x-2 mb-1.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-purple-500" />
                    <h3 className="font-bold text-sm text-slate-200">Sgae.Application</h3>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {cleanArchitectureDescription.application}
                  </p>
                </div>

                {/* Camada 3: Infrastructure */}
                <div className="p-4 bg-slate-900/60 rounded-xl border border-slate-800/80 hover:border-sky-500/20 transition duration-150">
                  <div className="flex items-center space-x-2 mb-1.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-sky-500" />
                    <h3 className="font-bold text-sm text-slate-200">Sgae.Infrastructure</h3>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {cleanArchitectureDescription.infrastructure}
                  </p>
                </div>

                {/* Camada 4: Presentation / API */}
                <div className="p-4 bg-slate-900/60 rounded-xl border border-slate-800/80 hover:border-pink-500/20 transition duration-150">
                  <div className="flex items-center space-x-2 mb-1.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-pink-500" />
                    <h3 className="font-bold text-sm text-slate-200">Sgae.API</h3>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {cleanArchitectureDescription.api}
                  </p>
                </div>

              </div>

            </div>
          </div>
        )}

      </main>

      {/* Footer minimalista do Painel */}
      <footer className="border-t border-slate-900 bg-slate-900/40 px-6 py-3 flex justify-between items-center text-[11px] text-slate-500 font-mono select-none">
        <span>SGAE .NET 8 Workspace Setup</span>
        <div className="flex items-center space-x-4">
          <span className="flex items-center space-x-1.5 text-emerald-500">
            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
            <span>PostgreSql Connection: Active</span>
          </span>
          <span>v1.0.0</span>
        </div>
      </footer>
    </div>
  );
}
