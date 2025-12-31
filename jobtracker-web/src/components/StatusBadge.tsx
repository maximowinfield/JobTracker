import type { ApplicationStatus } from "../api/jobApps";

const STATUS_CLASSES: Record<ApplicationStatus, string> = {
  Draft: "bg-slate-100 text-slate-700 ring-slate-200",
  Applied: "bg-blue-100 text-blue-800 ring-blue-200",
  Interviewing: "bg-amber-100 text-amber-800 ring-amber-200",
  Offer: "bg-green-100 text-green-800 ring-green-200",
  Accepted: "bg-emerald-100 text-emerald-800 ring-emerald-200",
  Rejected: "bg-red-100 text-red-800 ring-red-200",
};

export default function StatusBadge(props: { status: ApplicationStatus }) {
  const { status } = props;

  return (
    <span
      className={[
        "inline-flex items-center rounded-full px-2.5 py-1",
        "text-xs font-semibold tracking-wide whitespace-nowrap",
        "ring-1 ring-inset",
        STATUS_CLASSES[status],
      ].join(" ")}
    >
      {status}
    </span>
  );
}
