import { useState, MouseEvent } from "react";
import { useSession } from '@hook/session-provider';
import { usePagination } from '@hook/usePagination';
import { register } from "@registry";
import {
    listScenarioPagedSorted,
    deleteScenario,
    Scenario
} from '@api/scenario';
import { Spinner } from '@component/Spinner';
import { CreateScenarioModal } from './CreateScenarioModal';
import { ScenarioSession } from '../ScenarioSession';
import './style.scss';

const SORT_OPTIONS = [
    { value: 'createTime', label: 'Date created' },
    { value: 'title', label: 'Title' },
] as const

export const ScenariosIndexPage = () => {
    const { session } = useSession();

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isDeleting, setIsDeleting] = useState<string | null>(null);
    const [activeScenarioId, setActiveScenarioId] = useState<string | null>(null);

    const {
        page,
        size,
        nextPage,
        prevPage,
        results: scenarios,
        isLoading: scenariosLoading,
        error: scenariosError,
        sortField,
        sortDir,
        setSortField,
        setSortDir,
    } = usePagination<Scenario[]>(listScenarioPagedSorted, [isModalOpen, isDeleting]);

    const handleDelete = async (scenario: Scenario, e: MouseEvent) => {
        e.stopPropagation();
        setIsDeleting(scenario.id);
        try {
            await deleteScenario(scenario);
        } finally {
            setIsDeleting(null);
        }
    };

    if (scenariosLoading) {
        return <div className={'scenarios-status'}><Spinner /></div>;
    }

    if (scenariosError) {
        return (
            <div className={'scenarios-status'}>
                <div className={'error'}>{`Couldn't load scenarios: ${scenariosError}`}</div>
            </div>
        );
    }

    const cards = scenarios ?? [];
    const mayHaveNextPage = cards.length === size;

    const formatTime = (iso?: string): string => {
        if (!iso) return '';
        const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86_400_000);
        if (days <= 0) return 'Today';
        if (days === 1) return 'Yesterday';
        if (days < 30) return `${days}d ago`;
        return `${Math.floor(days / 30)}mo ago`;
    };

    return (
        <div className={'scenarios'}>
            <div className={'scenarios-header'}>
                <h1>Scenarios</h1>
            </div>

            <div className={'card-actions'}>
                <div className={'pagination'}>
                    <button onClick={prevPage} disabled={page <= 1}>Previous</button>
                    <span>{`Page ${page}`}</span>
                    <button onClick={nextPage} disabled={!mayHaveNextPage}>Next</button>
                </div>

                <div className={'sort-controls'}>
                    <select value={sortField ?? ''} onChange={(e) => setSortField(e.target.value || undefined)}>
                        {SORT_OPTIONS.map((opt) => (
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                    </select>
                    <select value={sortDir ?? 'desc'} onChange={(e) => setSortDir(e.target.value)}>
                        <option value="asc">Asc</option>
                        <option value="desc">Desc</option>
                    </select>
                </div>

                <div className={'primary-actions'}>
                    <button className={'create-button'} onClick={() => setIsModalOpen(true)}>
                        Create new
                    </button>
                </div>
            </div>

            {cards.length === 0 && (
                <div className={'notice'}>
                    <div className={'notice-title'}>No scenarios yet</div>
                    <div className={'notice-body'}>Click "Create new" to add your first scenario.</div>
                </div>
            )}

            {cards.length > 0 && (
                <div className={'scenario-grid'}>
                    {cards.map((scenario) => (
                        <div
                            key={scenario.id}
                            className={'scenario-card'}
                            onClick={() => setActiveScenarioId(scenario.id)}
                            role={'button'}
                            tabIndex={0}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault();
                                    setActiveScenarioId(scenario.id);
                                }
                            }}
                        >
                            <div className={'card-header'}>
                                <h3 className={'scenario-title'}>{scenario.title}</h3>
                                <button
                                    className={'delete-button'}
                                    onClick={(e) => handleDelete(scenario, e)}
                                    disabled={isDeleting === scenario.id}
                                    aria-label={'Delete scenario'}
                                >
                                    {isDeleting === scenario.id ? '...' : '✕'}
                                </button>
                            </div>

                            <div className={'card-body'}>
                                <div className={'steps-preview'}>
                                    {scenario.steps.split('\n').filter(Boolean).slice(0, 3).map((step, index) => (
                                        <div key={index} className={'step-item'}>
                                            <span className={'step-number'}>{index + 1}.</span>
                                            <span className={'step-text'}>{step}</span>
                                        </div>
                                    ))}
                                    {scenario.steps.split('\n').filter(Boolean).length > 3 && (
                                        <div className={'more-steps'}>
                                            +{scenario.steps.split('\n').filter(Boolean).length - 3} more steps
                                        </div>
                                    )}
                                </div>
                            </div>

                            <div className={'card-footer'}>
                                <span className={'create-time'}>{formatTime(scenario.createTime)}</span>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {isModalOpen && (
                <CreateScenarioModal
                    onClose={() => setIsModalOpen(false)}
                    onCreated={() => setIsModalOpen(false)}
                />
            )}

            {activeScenarioId && (
                <ScenarioSession
                    scenarioId={activeScenarioId}
                    onClose={() => setActiveScenarioId(null)}
                />
            )}
        </div>
    );
};

register('@page/scenarios-page', ScenariosIndexPage);
