import type { ReactNode } from "react";
import { Icon, type IconName } from "./Icon";

type PageHeaderProps = {
  icon: IconName;
  title: string;
  subtitle?: string;
  actions?: ReactNode;
};

export const PageHeader = ({ icon, title, subtitle, actions }: PageHeaderProps) => {
  return (
    <header className="admin-page-header">
      <div>
        <h2 className="admin-page-title">
          <Icon name={icon} />
          {title}
        </h2>
        {subtitle && <p className="admin-page-subtitle">{subtitle}</p>}
      </div>
      {actions}
    </header>
  );
};
