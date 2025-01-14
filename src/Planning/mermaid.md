
```mermaid
graph TD
    %% States
    subgraph State1["State 1"]
        W1["World 1<br>Predicates: {P, Q}<br>Cost: Obj=2, Sub=3"] -->|Action: A| W2["World 2<br>Predicates: {P, R}<br>Cost: Obj=3, Sub=4"]
        W1 -->|Agent: Alice| W2
        W1 -.->|Agent: Bob| W3["World 3<br>Predicates: {Q, R}<br>Cost: Obj=4, Sub=5 (Pruned)"]
    end

    subgraph State2["State 2"]
        W4["World 4<br>Predicates: {P, S}<br>Cost: Obj=1, Sub=2"] -->|Action: B| W5["World 5<br>Predicates: {P, Q, S}<br>Cost: Obj=2, Sub=3"]
    end

    %% State Relationships
    State1 -->|Parent| State2

    %% Accessibility Edges Between Worlds
    W2 -.->|Agent: Alice| W4
    W3 -.->|Agent: Bob| W5
  ```
