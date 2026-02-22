import type { ReactNode } from "react";

type ActionRowProps = {
  children: ReactNode;
  className?: string;
};

export const ActionRow = ({ children, className }: ActionRowProps) => {
  const classes = className ? `ui-action-row ${className}` : "ui-action-row";
  return <div className={classes}>{children}</div>;
};
