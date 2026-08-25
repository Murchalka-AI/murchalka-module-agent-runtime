# ADR 0001: Agent Runtime contract and state

Status: Accepted

The module owns `agent.turn@1`. The capability is stateless and owns no canonical persistence. Runtime interaction is bounded, cancellation-aware, default-deny, and uses only manifest-granted dependencies.

