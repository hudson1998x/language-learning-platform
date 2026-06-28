import {useState} from "react";
import {useSession} from '@hook/session-provider'
import {useLlmCapability} from '@hook/llm-capability-provider'
import "./style.scss";

export interface NavLink {
    label: string;
    href: string;
    requiresLlm?: boolean;
}

interface NavBarProps {
    links?: NavLink[];
    initialActive?: string;
}

const LINKS: NavLink[] = [
    {label: "Dashboard", href: "/"},
    {label: "Flash cards", href: "/flashcards"},
    {label: "Music translation", href: "/musiclyrics", requiresLlm: true},
    // { label: "Leddit", href: "/leddit" }, // disabled for now, future feature.
    {label: "LeMessage", href: "/messages", requiresLlm: true}
];

export const NavBar = ({links = [...LINKS], initialActive = links[0]?.href}: NavBarProps) => {

    const active = links.filter((l) => l.href == location.pathname)?.[0]?.href;
    const {isAdmin} = useSession();
    const {available: llmAvailable} = useLlmCapability();

    if (isAdmin) {
        links.push({label: 'App settings', href: "/settings"});
    }

    const filterWhereNoLlm = (link: NavLink) => {
        if (llmAvailable) {
            return true;
        }
        if (!link.requiresLlm) {
            return true;
        }
        return false;
    }

    return (
        <nav className="navbar">
            <ul className="navbar__list">
                {links.filter(filterWhereNoLlm).map((link) => (
                    <li key={link.href} className="navbar__item">
                        <a
                            href={link.href}
                            className={`navbar__link ${active === link.href ? "navbar__link--active" : ""}`}
                            aria-current={active === link.href ? "page" : undefined}
                        >
                            {link.label}
                        </a>
                    </li>
                ))}
            </ul>
        </nav>
    );
};