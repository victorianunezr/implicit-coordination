
```mermaid
graph LR
  %% Subgraph for State #1
  subgraph S1["State #1"]
    w1["W1<br/>{p, q}<br/>obj=2, subj=4"]:::notPruned
    w2["W2<br/>{r}<br/>obj=∞, subj=∞"]:::pruned

    %% Accessibility edge within State #1
    w1 --|agentA| w2
  end

  %% Subgraph for State #2
  subgraph S2["State #2"]
    w3["W3<br/>{p}<br/>obj=1, subj=2"]:::notPruned
  end

  %% Parent-child relationship (action label)
  S1 -->|action: A1| S2

  %% Define style classes for pruned vs. notPruned
  classDef notPruned fill:#fff,stroke:#000,stroke-width:1px
  classDef pruned stroke-dasharray:3 3,fill:#eee,color:#888
  ```
