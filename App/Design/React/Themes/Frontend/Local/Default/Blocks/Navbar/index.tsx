import { useState } from "react";
import "./style.scss";

export interface NavLink {
    label: string;
    href: string;
}

interface NavBarProps {
    links?: NavLink[];
    initialActive?: string;
}

const LINKS: NavLink[] = [
    { label: "Dashboard", href: "/" },
    { label: "Flash cards", href: "/flashcards" },
    { label: "Music translation", href: "/musiclyrics" },
    { label: "Scenarios", href: "/scenarios" },
    // { label: "Leddit", href: "/leddit" }, // disabled for now, future feature.
    { label: "LeMessage", href: "/messages"}
];

export const NavBar = ({ links = LINKS, initialActive = links[0]?.href }: NavBarProps) => {
    const active = links.filter((l) => l.href == location.pathname)?.[0]?.href;

    return (
        <nav className="navbar">
            <ul className="navbar__list">
                {links.map((link) => (
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