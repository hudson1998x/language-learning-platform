import { FC } from "react";

export type TextProps = {
    text: string
}

export const Text: FC<TextProps> = (props: TextProps) => {
    return (<p>{props.text}</p>)
}