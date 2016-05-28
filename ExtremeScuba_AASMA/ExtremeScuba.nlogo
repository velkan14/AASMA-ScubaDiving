globals [
  iteration

  interation-actions
  simulation-actions
]

breed [divers diver]
breed [gambozinos gambozino]
breed [urchins urchin]
breed [bubbles bubble]

turtles-own [
  hit-by

  attacked-by

  helped-by

  rest

  max-speed

  iterations

  gambuzinos-captured-last-iteration

  observed-actions

  incoming-qeue

  my-type
]

divers-own [
  health
  oxygen
  gambozinos-caught
  gambozinos-in-the-backpack

  got;;
  actions-box;;

  agent-architecture

  visible-bubbles ;;FIXME: Deveria ser uma lista e não um agent-set
  visible-gambozinos
  visible-urchins
  visible-divers

  ;;turtles that the diver know about because of received messages
  known-bubbles
  known-gambozinos
  known-urchins
  known-divers

  ;;Things for BDI
  last-action
  plan
  intention
  desire

  ;;Things for emotions
  love
  loved
  anger
  angered
  fear
  feared
  sadness
  happiness
]

to setup
  clear-turtles
  clear-all

  ask patches [ set pcolor 102 ] ;; dark blue = 102

  set iteration 0
  set interation-actions (list)
  set simulation-actions (list)

  set-default-shape bubbles "circle 3"
  set-default-shape gambozinos "fish"
  set-default-shape urchins "urchin"
  set-default-shape divers "person"

  init-bubbles num-bubbles
  init-gambozinos num-gambozinos
  init-urchins num-urchins
  init-divers num-divers

  reset-ticks
end

;; --------------------------- GO -------------------------------------
to go
  tick
  ask patches [set pcolor 102]
  ask bubbles [bubbles-loop]
  ask gambozinos [gambozinos-loop]
  ask urchins [urchins-loop]
  ask divers [divers-loop]

  create-random

  if count divers <= 0 or iteration > iterations-for-expedition [stop]
  set iteration iteration + 1

end
;; ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||| GO

;;INICIALIZATION OF TURTLES

to init-bubbles [ num ]
  create-bubbles num [
    set color 87 ;; light cyan = 87
    setxy random-pxcor random-pycor
    set max-speed 0
    set my-type "bubble"
    init-heading

  ]
end

to init-gambozinos [ num ]
  create-gambozinos num [
    set hit-by (list)
    set attacked-by (list)
    set max-speed gambozinos-speed
    set color yellow
    setxy random-pxcor random-pycor
    init-heading
    set my-type "gambozino"
    set rest 0
  ]
end

to init-urchins [ num ]
  create-urchins num [
    set hit-by (list)
    set attacked-by (list)
    set max-speed urchins-speed
    set color magenta + 1
    setxy random-pxcor random-pycor
    init-heading
    set my-type "urchin"
    set rest 0
  ]
end

;;INICIALIZATION OF DIVERS

to init-divers [ num ]
  create-divers num [
    set health 100
    set oxygen 100
    set gambozinos-caught 0
    set gambozinos-in-the-backpack 0

    set got (list)
    set actions-box (list)

    set incoming-qeue (list)
    set hit-by (list)
    set attacked-by (list)
    set helped-by (list)
    set max-speed divers-speed

    init-heading

    set color red
    setxy random-pxcor random-pycor
    set label (word "HP:" health "; O2:" oxygen "; Caught:" gambozinos-caught)

    set visible-bubbles (no-turtles)
    set visible-gambozinos (no-turtles)
    set visible-urchins (no-turtles)
    set visible-divers (no-turtles)

    set known-bubbles (list)
    set known-gambozinos (list)
    set known-urchins (list)
    set known-divers (list)

    set agent-architecture architecture

    set last-action ""
    set plan build-empty-plan
    set rest 0

    set my-type "diver"

    set love 0
    set anger 0
    set fear 0
    set sadness 0
    set happiness 0
    set loved (list)
    set angered (list)
    set feared (list)
  ]
end

to init-heading
  let r random 4
  ifelse r = 0 [set heading 0]
  [ifelse r = 1 [set heading 90]
    [ifelse r = 2[set heading 180]
      [ifelse r = 3[set heading 270]
        [set heading 360]]]]
end

to update-visible-divers
  let id who
  set visible-divers (divers in-cone max-distance max-angle) with [who != id]
end

to update-visible-bubbles
  let id who
  set visible-bubbles (bubbles in-cone max-distance max-angle)
end

to update-visible-urchins
  let id who
  set visible-urchins (urchins in-cone max-distance max-angle)
end

to update-visible-gambozinos
  let id who
  set visible-gambozinos (gambozinos in-cone max-distance max-angle)
end

to update-known
  let id who
  let u known-urchins
  ask visible-urchins [set u add-new-struct u self id]
  set known-urchins u

  let d known-divers
  ask visible-divers [set d add-new-struct d self id]
  set known-divers d

  let g known-gambozinos
  ask visible-gambozinos [set g add-new-struct g self id]
  set known-gambozinos g

  let b known-bubbles
  ask visible-bubbles [set b add-new-struct b self id]
  set known-bubbles b
end
;;DIVER SENSORES

;;FIXME: verificar estes closes
to-report close-to-diver?
  report any? visible-divers
end

to-report close-to-gambozino?
  report any? visible-gambozinos
end

to-report close-to-urchin?
  report any? visible-urchins
end

to-report close-to-bubble?
  report any? (bubbles in-cone harpon-distance max-angle)
end

to-report can-attack? [a]
  if distance a < harpon-distance [report true]
  report false
end

to-report can-attack-gambozinos?
  ifelse any? (gambozinos in-cone harpon-distance max-angle) [report true]
  [report false]
end

to-report can-attack-urchins?
  ifelse any? (urchins in-cone harpon-distance max-angle) [report true]
  [report false]
end

to-report harpon-hit?
  let r random 1
  if  r < probability-of-hit [report true]
  report false
end

to-report is-oxygen-zero?
  report oxygen <= 0
end

to-report is-health-zero?
  report health <= 0
end

to-report is-low-oxygen?
  report oxygen < 40
end

to-report is-low-health?
  report health < 40
end

to-report patch-ahead-clear?
  report true

  let result 0
  ask patch-ahead 1 [set result not any? turtles-here]
  report result
end

;;DIVER ACTUATORS

to communicate
  let d known-divers
  let g known-gambozinos
  let u known-urchins
  let b known-bubbles
  ask one-of (visible-divers) [
    foreach d [set known-divers add-to-struct known-divers ? ]
    foreach g [set known-gambozinos add-to-struct known-gambozinos ?]
    foreach u [set known-urchins add-to-struct known-urchins ?]
    foreach b [set known-bubbles add-to-struct known-bubbles ?]
   ]
end

to attack [animal]
  if animal != nobody[
    if harpon-hit?
    [
      ask animal [die]
      caught-animal
    ]
  ]
end

to attack-gambozino
  let g min-one-of (gambozinos in-cone harpon-distance max-angle) [distance myself]
  if g != nobody[
    if harpon-hit?
    [
      let id 0
      ask g [set id who die]
      remove-known-gambozinos id
      caught-animal

    ]
  ]
end

to attack-urchin
  let u min-one-of (urchins in-cone harpon-distance max-angle) [distance myself]
  if u != nobody[
    if harpon-hit?
    [
      let id 0
      ask u [set id who die]
      remove-known-urchins id
      caught-animal

    ]
  ]
end

to take-bubble
  let bubble min-one-of (bubbles in-cone harpon-distance max-angle) [distance myself]
  set oxygen 100
  ask bubble [die]
end

to caught-animal
  set gambozinos-caught gambozinos-caught + 1
  set gambozinos-in-the-backpack gambozinos-in-the-backpack + 1
end

to lose-health [n]
  set health health - n
end

;;URCHINS SENSORES

to-report touching-diver?
  report any? (divers in-cone urchin-distance 360)
end

;;URCHINS ACTUATORS
to attack-diver
  let diver one-of (divers in-cone urchin-distance 360)
  ask diver [set health health - 5]
end

to rotate-random
  let rand (random 2)
  ifelse(rand = 0)
  [rt 90]
  [rt -90]
end

to move
  ifelse rest = max-speed
     [
       fd 1
       set rest 0
     ][
       set rest rest + 1
     ]
end

;; LOOPS

to divers-loop
    set oxygen oxygen - oxygen-decay
  set label (word "HP:" health "; O2:" oxygen "; Caught:" gambozinos-caught)

  if is-oxygen-zero? [die]
  if is-health-zero? [die]

  update-visible-divers
  update-visible-bubbles
  update-visible-gambozinos
  update-visible-urchins
  update-known
  if close-to-diver? [communicate]
  print known-bubbles
  ifelse architecture = "reactive" [divers-reactive-loop]
  [if architecture = "deliberative BDI"[divers-deliberative-BDI-loop]]

  ask (patches in-cone max-distance max-angle) [set pcolor blue]
  ask (patches in-cone harpon-distance max-angle) [set pcolor yellow]
end

to divers-reactive-loop

  ifelse close-to-bubble? and is-low-oxygen? [take-bubble]
  [
    ifelse can-attack-urchins? [attack-urchin]
    [
      ifelse can-attack-gambozinos? [attack-gambozino]
      [
        ifelse patch-ahead-clear? and random-float 1 < 0.8 [ move]
        [rotate-random]
        ]]]
end


to divers-deliberative-BDI-loop
  ;;if goal-succeeded?
    ;;[stop]

  set last-action ""
  ifelse not (empty-plan? plan or intention-succeeded? intention or impossible-intention? intention)
  [
    execute-plan-action
    update-beliefs
  ]
  [
    update-beliefs
    ;; Check the robot's options
    set desire BDI-options
    set intention BDI-filter
    set plan build-plan-for-intention intention
    ;;print plan
    ;; If it could not build a plan, the robot should behave as a reactive agent
    if(empty-plan? plan)
      [divers-reactive-loop ]
  ]
end

;;;
;;;  Execute the next action of the current plan
;;;
to execute-plan-action
  let currentInstruction 0

  set currentInstruction get-plan-first-instruction plan

  ifelse(instruction-caught-oxygen? currentInstruction)
  [
    if close-to-bubble? [take-bubble]
    set plan remove-plan-first-instruction plan
  ]
  [ ifelse(instruction-caught-gambozinos? currentInstruction)
    [
      attack-gambozino
      set plan remove-plan-first-instruction plan
    ]
    [ ifelse(instruction-run-from-urchins? currentInstruction)
      [
        attack-urchin
        set plan remove-plan-first-instruction plan
      ]
      [ if(instruction-find-adjacent-position? currentInstruction)
      [
        let x 0
        let y 0
        ask patch-ahead 1 [set x pxcor set y pycor]
        let p (list x y)
        ifelse(p = get-instruction-value currentInstruction)
        [
          ifelse patch-ahead-clear? [move
            set plan remove-plan-first-instruction plan]
        [ rt -90 ]
        ] [ rt 90 ]
      ]
      ]
    ]
  ]
end

;;;
;;;  Check if the goal has been achieved ( all boxes on shelves and robot on their initial positions)
;;;
to-report goal-succeeded?
  report false ;;( equal-positions? current-position build-position 0 0
          ;;and (heading = 90)
          ;;and (boxes-on-shelves = NUM_BOXES) )
end


to bubbles-loop
end

to gambozinos-loop
  ifelse patch-ahead-clear? and random-float 1 < .8 [move]
                            [rotate-random]
end

to urchins-loop
  ifelse touching-diver? [attack-diver]
  [

    ifelse patch-ahead-clear? and random-float 1 < .8 [move]
                              [rotate-random]
    ]
  ask (patches in-cone urchin-distance max-angle) [set pcolor pink]


end

to create-random
  let r random-float 1
  if r < probability-of-new-urchin [init-urchins 1]
  set r random-float 1
  if r < probability-of-new-bubble [init-bubbles 1]
  set r random-float 1
  if r < probability-of-new-gambozino [init-gambozinos 1]
end

;;;
;;; -------------------------
;;;    Plans
;;; -------------------------
;;;
to-report build-empty-plan
  report []
end

to-report add-instruction-to-plan [pplan iinstruction]
  report lput iinstruction pplan
end

to-report remove-plan-first-instruction [pplan]
  report butfirst pplan
end

to-report get-plan-first-instruction [pplan]
  report first pplan
end

to-report empty-plan? [pplan]
  report empty? pplan
end

;;;
;;; -------------------------
;;; Intention
;;;
;;; Chap.4 of [Wooldridge02]
;;; An intention is a list such as [desire position heading]
;;; -------------------------
;;;
to-report build-empty-intention
  report []
end

to-report build-intention [ddesire aagent]
  let aux 0

  set aux list ddesire aagent
  report aux
end

to-report get-intention-desire [iintention]
  report item 0 iintention
end

to-report get-intention-agent [iintention]
  report item 1 iintention
end

to-report empty-intention? [iintention]
  report empty? iintention
end

;;;
;;;  Check if the robot's intention has been achieved
;;;
to-report intention-succeeded? [iintention]
  let ddesire 0


  if(empty-intention? iintention)
    [ report false ]

  set ddesire get-intention-desire iintention
  ifelse(ddesire = "caught-oxygen")[ report oxygen = 100 ]
   [ifelse(ddesire = "run-from-urchins")
    [ report not close-to-urchin?]
    [if(ddesire = "caught-gambozinos")
      [ report get-intention-agent iintention = nobody] ;;FIXME
    ]
  ]
end

;;;
;;;  Check if an intention cannot be achieved anymore
;;;  However, in this scenario, the only intention that can become impossible is "grab", which is already tested in 'execute-plan-action'
;;;
to-report impossible-intention? [iintention]
  let ddesire get-intention-desire iintention
  if ddesire = "caught-gambozinos" and is-low-oxygen? [report true]
  if ddesire = "caught-gambozinos" and is-low-health? and any? (urchins in-cone harpon-distance 120) [report true]
  report false
end

;;;
;;;  Update the robot's beliefs based on its perceptions
;;;  Reference: Chap.4 of [Wooldridge02]
;;;
to update-beliefs
  update-visible-divers
  update-visible-bubbles
  update-visible-gambozinos
  update-visible-urchins
end

;;;
;;; According to the current beliefs, it selects the robot's desires
;;; Its values can be "caught-oxygen", "run-from-urchins" or "caught-gambozinos"
;;; Reference: Chap.4 de [Wooldridge02]
;;;
to-report BDI-options

  ifelse is-low-oxygen? [report "caught-oxygen"]
  [
    ifelse is-low-health? and close-to-urchin? [report "run-from-urchins"]
    [
      report "caught-gambozinos"
    ]
  ]
end

;;;
;;; It selects a desire and coverts it into an intention
;;; Reference: Chap.4 de [Wooldridge02]
;;;
to-report BDI-filter
  let objective 0
  ifelse desire = "caught-oxygen"
  [
    set objective min-one-of visible-bubbles [distance myself]
    ;;if objective = nobody [set objective min-one-of known-bubbles [distance myself]]
    ;;if objective = nobody [report build-empty-intention]
    ifelse objective = nobody[
      if empty? known-bubbles [report build-empty-intention]
      set objective one-of known-bubbles
    ]
    [let id 0
          ask objective [set id who]
          set objective get-bubble-struct id]

    report build-intention desire objective
  ]
  [
    ifelse desire = "run-from-urchins"
    [
      set objective min-one-of visible-urchins [distance myself]
      ;;if objective = nobody [set objective min-one-of known-urchins [distance myself]]
      ;;if objective = nobody [report build-empty-intention]
      ifelse objective = nobody[
        if empty? known-urchins [report build-empty-intention]
        set objective one-of known-urchins
      ]
      [let id 0
          ask objective [set id who]
          set objective get-urchin-struct id]

      report build-intention desire objective
    ]
    [
      if desire = "caught-gambozinos"
      [
        set objective min-one-of visible-gambozinos [distance myself]
        ;;if objective = nobody [set objective min-one-of known-gambozinos [distance myself]]
        ifelse objective = nobody[
          if empty? known-gambozinos [report build-empty-intention]
          set objective one-of known-gambozinos
        ]
        [ let id 0
          ask objective [set id who]
          set objective get-gambozino-struct id]


        report build-intention desire objective
      ]
    ]
  ]
  report build-empty-intention
end

to-report get-adj-cors [a]
  report (list get-xcor-struct a get-ycor-struct a)
  ;;report (list (x + 1) (y + 1))
end

to-report get-far-cors [a]
  let x get-xcor-struct a
  let y get-ycor-struct a

  let xdist x - xcor
  let ydist y - ycor
  if xdist != 0 [ifelse xdist < 0 [set xdist -1][set xdist 1]]
  if ydist != 0 [ifelse ydist < 0 [set ydist -1][set ydist 1]]
  report (list (xcor - xdist * 2) (ycor - ydist * 2))
end

;;;
;;;  Create a plan for a given intention
;;;
to-report build-plan-for-intention [iintention]
  let new-plan 0

  set new-plan build-empty-plan

  if  not empty-intention? iintention
  [
    let aagent get-intention-agent iintention

    ifelse get-intention-desire iintention = "run-from-urchins"
    [
      set new-plan build-path-plan (list xcor ycor) get-far-cors aagent
      set new-plan add-instruction-to-plan new-plan build-instruction-run-from-urchins get-id-struct aagent
    ]
    [
      set new-plan build-path-plan (list xcor ycor) get-adj-cors aagent

      if get-intention-desire iintention = "caught-oxygen"
      [
        set new-plan add-instruction-to-plan new-plan build-instruction-caught-oxygen get-id-struct aagent
      ]

      if get-intention-desire iintention = "caught-gambozinos"
      [
        set new-plan add-instruction-to-plan new-plan build-instruction-caught-gambozinos get-id-struct aagent
      ]
    ]
  ]

  report new-plan
end

;;;
;;; Build a pan to move the agent from posi to posf
;;;
to-report build-heading-plan [aagent]
  let newPlan 0
  let headingPlan 0


  set newPlan build-empty-plan

  set headingPlan (towards aagent)
  set newPlan add-instruction-to-plan newPlan build-instruction-find-heading headingPlan

  report newPlan
end

to-report build-path-plan [posi posf]
  let newPlan 0
  let path 0


  set newPlan build-empty-plan
  set path (find-path posi posf)
  foreach path
    [ set newPlan add-instruction-to-plan newPlan build-instruction-find-adjacent-position ? ]

  report newPlan
end


;;;
;;; -------------------------
;;;    Plan Intructions
;;; -------------------------
;;;
to-report build-instruction [ttype vvalue]
  report list ttype vvalue
end

to-report get-instruction-type [iinstruction]
  report first iinstruction
end

to-report get-instruction-value [iinstruction]
  report last iinstruction
end

to-report build-instruction-find-heading [hheading]
  report build-instruction "h" hheading
end

to-report build-instruction-caught-oxygen [id]
  report build-instruction "co" id
end

to-report build-instruction-run-from-urchins [id]
  report build-instruction "rfu" id
end

to-report build-instruction-caught-gambozinos [id]
  report build-instruction "cg" id
end

to-report instruction-find-adjacent-position? [iinstruction]
  report get-instruction-type iinstruction = "ad"
end

to-report instruction-caught-oxygen? [iinstruction]
  report get-instruction-type iinstruction = "co"
end

to-report instruction-caught-gambozinos? [iinstruction]
  report get-instruction-type iinstruction = "cg"
end

to-report instruction-run-from-urchins? [iinstruction]
  report get-instruction-type iinstruction = "rfu"
end

to-report build-instruction-find-adjacent-position [aadjacent-position]
  report build-instruction "ad" aadjacent-position
end

;;;
;;;  Return a list of positions from initialPos to FinalPos
;;;  The returning list excludes the initialPos
;;;  If no path is found, the returning list is empty
;;;
to-report find-path [intialPos FinalPos]
  let opened 0
  let closed 0
  let aux 0
  let aux2 0
  let aux3 0
  let to-explore 0

  set to-explore []
  set closed []
  set opened []
  set opened fput (list (list 0 0 intialPos) []) opened

  while [not empty? opened]
  [
    set to-explore first opened
    set opened remove to-explore opened
    set closed fput to-explore closed

    ifelse last first to-explore = FinalPos
    [ report find-solution to-explore closed ]
    [ set aux adjacents to-explore FinalPos
      foreach aux
      [
        set aux2 ?
        set aux3 filter [ last first aux2 = last first ? and first first aux2 < first first ? ] opened
        ifelse not empty? aux3
        [ set opened remove first aux3 opened
          set opened fput aux2 opened ]
        [
          set aux3 filter [ last first aux2 = last first ? ] closed
          ifelse not empty? aux3
          [
            if first first first aux3 > first first aux2
              [ set closed remove first aux3 closed
                set opened fput aux2 opened ]
          ]
          [ set opened fput aux2 opened ]
        ]
      ]

      ;; orders the opened list according to the heuristic
      set opened sort-by [ first first ?1 < first first ?2 ] opened
    ]
  ]
  report []
end

to-report find-solution [node closed]
  let solution 0
  let parent 0

  set solution (list last first node)
  set parent item 1 node
  while [not empty? parent] [
    set parent first filter [ parent = first ? ] closed
    set solution fput last first parent solution
    set parent last parent
  ]

  report butfirst solution
end

to-report adjacents [node mobjectivo]
  let aux 0
  let aux2 0

  set aux2 []
  set aux adjacent-positions-of-type (last first node)
  foreach aux [ set aux2 fput (list 0
                                  ((item 1 first node) + 1)
                                   ?)
                             aux2 ]
  set aux []
  foreach aux2
  [ set aux fput (list (replace-item 0 ? (heuristic ? mobjectivo))
                       first node)
                 aux ]
  report aux
end

to-report adjacent-positions-of-type [pos]
  let solution 0
  let x 0
  let y 0

  set x item 0 pos
  set y item 1 pos

  set solution []
  set solution fput (list x (y - 1)) solution
  set solution fput (list x (y + 1)) solution
  set solution fput (list (x - 1) y) solution
  set solution fput (list (x + 1) y) solution
  report solution
end


;;;
;;;  Add the distance to the goal position and the current node cost
;;;
to-report heuristic [node mgoal]
  let cost 0
  let x 0
  let y 0

  set cost item 1 node
  set x first item 2 node
  set y first butfirst item 2 node

  report cost +
         2 * (abs(x - item 0 mgoal) +  abs(y - item 1 mgoal))
end

;;;
;;;  EMOTIONS -------------------------------------------------------
;;;

__includes[
  "emotions.nls"
  "struct.nls"
  ]
@#$#@#$#@
GRAPHICS-WINDOW
349
114
1242
618
34
18
12.8
1
10
1
1
1
0
1
1
1
-34
34
-18
18
1
1
1
ticks
30.0

BUTTON
164
382
230
415
NIL
setup
NIL
1
T
OBSERVER
NIL
NIL
NIL
NIL
1

BUTTON
237
382
300
415
go
go
T
1
T
OBSERVER
NIL
NIL
NIL
NIL
1

INPUTBOX
493
19
581
79
num-bubbles
20
1
0
Number

INPUTBOX
691
19
798
79
num-divers
10
1
0
Number

INPUTBOX
588
19
684
79
num-urchins
5
1
0
Number

INPUTBOX
371
20
483
80
num-gambozinos
10
1
0
Number

INPUTBOX
121
106
237
166
gambozinos-speed
4
1
0
Number

INPUTBOX
243
106
338
166
urchins-speed
4
1
0
Number

INPUTBOX
9
105
115
165
divers-speed
0
1
0
Number

CHOOSER
208
30
357
75
architecture
architecture
"reactive" "deliberative BDI" "BDI w/ emotions"
1

INPUTBOX
90
216
181
276
harpon-distance
1
1
0
Number

SLIDER
977
28
1149
61
probability-of-hit
probability-of-hit
0
1
0.8
0.1
1
NIL
HORIZONTAL

INPUTBOX
10
216
87
276
max-distance
4
1
0
Number

SLIDER
10
282
182
315
max-angle
max-angle
0
360
360
1
1
º
HORIZONTAL

MONITOR
11
349
99
394
Gambozinos
count gambozinos
17
1
11

MONITOR
11
394
98
439
NIL
count urchins
17
1
11

SLIDER
10
316
182
349
oxygen-decay
oxygen-decay
0
100
1
1
1
NIL
HORIZONTAL

SLIDER
11
480
203
513
probability-of-new-gambozino
probability-of-new-gambozino
0
1
0.1
0.1
1
NIL
HORIZONTAL

SLIDER
12
445
203
478
probability-of-new-bubble
probability-of-new-bubble
0
1
0
0.1
1
NIL
HORIZONTAL

SLIDER
12
517
204
550
probability-of-new-urchin
probability-of-new-urchin
0
1
0.1
0.1
1
NIL
HORIZONTAL

INPUTBOX
11
560
202
620
iterations-for-expedition
1000
1
0
Number

TEXTBOX
11
89
161
107
The bigger the slower:
11
0.0
1

INPUTBOX
182
216
267
276
urchin-distance
1
1
0
Number

PLOT
9
631
648
984
Turtles count
iteration
Num
0.0
1000.0
0.0
100.0
true
true
"" ""
PENS
"divers" 1.0 0 -15040220 true "" "plot count divers"
"gambozinos" 1.0 0 -13791810 true "" "plot count gambozinos"
"urchins" 1.0 0 -5825686 true "" "plot count urchins"
"bubbles" 1.0 0 -16777216 true "" "plot count bubbles"

@#$#@#$#@
## WHAT IS IT?

(a general understanding of what the model is trying to show or explain)

## HOW IT WORKS

(what rules the agents use to create the overall behavior of the model)

## HOW TO USE IT

(how to use the model, including a description of each of the items in the Interface tab)

## THINGS TO NOTICE

(suggested things for the user to notice while running the model)

## THINGS TO TRY

(suggested things for the user to try to do (move sliders, switches, etc.) with the model)

## EXTENDING THE MODEL

(suggested things to add or change in the Code tab to make the model more complicated, detailed, accurate, etc.)

## NETLOGO FEATURES

(interesting or unusual features of NetLogo that the model uses, particularly in the Code tab; or where workarounds were needed for missing features)

## RELATED MODELS

(models in the NetLogo Models Library and elsewhere which are of related interest)

## CREDITS AND REFERENCES

(a reference to the model's URL on the web if it has one, as well as any other necessary credits, citations, and links)
@#$#@#$#@
default
true
0
Polygon -7500403 true true 150 5 40 250 150 205 260 250

airplane
true
0
Polygon -7500403 true true 150 0 135 15 120 60 120 105 15 165 15 195 120 180 135 240 105 270 120 285 150 270 180 285 210 270 165 240 180 180 285 195 285 165 180 105 180 60 165 15

arrow
true
0
Polygon -7500403 true true 150 0 0 150 105 150 105 293 195 293 195 150 300 150

box
false
0
Polygon -7500403 true true 150 285 285 225 285 75 150 135
Polygon -7500403 true true 150 135 15 75 150 15 285 75
Polygon -7500403 true true 15 75 15 225 150 285 150 135
Line -16777216 false 150 285 150 135
Line -16777216 false 150 135 15 75
Line -16777216 false 150 135 285 75

bug
true
0
Circle -7500403 true true 96 182 108
Circle -7500403 true true 110 127 80
Circle -7500403 true true 110 75 80
Line -7500403 true 150 100 80 30
Line -7500403 true 150 100 220 30

butterfly
true
0
Polygon -7500403 true true 150 165 209 199 225 225 225 255 195 270 165 255 150 240
Polygon -7500403 true true 150 165 89 198 75 225 75 255 105 270 135 255 150 240
Polygon -7500403 true true 139 148 100 105 55 90 25 90 10 105 10 135 25 180 40 195 85 194 139 163
Polygon -7500403 true true 162 150 200 105 245 90 275 90 290 105 290 135 275 180 260 195 215 195 162 165
Polygon -16777216 true false 150 255 135 225 120 150 135 120 150 105 165 120 180 150 165 225
Circle -16777216 true false 135 90 30
Line -16777216 false 150 105 195 60
Line -16777216 false 150 105 105 60

car
false
0
Polygon -7500403 true true 300 180 279 164 261 144 240 135 226 132 213 106 203 84 185 63 159 50 135 50 75 60 0 150 0 165 0 225 300 225 300 180
Circle -16777216 true false 180 180 90
Circle -16777216 true false 30 180 90
Polygon -16777216 true false 162 80 132 78 134 135 209 135 194 105 189 96 180 89
Circle -7500403 true true 47 195 58
Circle -7500403 true true 195 195 58

circle
false
0
Circle -7500403 true true 0 0 300

circle 2
false
0
Circle -7500403 true true 0 0 300
Circle -16777216 true false 30 30 240

circle 3
false
0
Circle -7500403 true true 0 0 300
Circle -1 true false 30 30 240

cow
false
0
Polygon -7500403 true true 200 193 197 249 179 249 177 196 166 187 140 189 93 191 78 179 72 211 49 209 48 181 37 149 25 120 25 89 45 72 103 84 179 75 198 76 252 64 272 81 293 103 285 121 255 121 242 118 224 167
Polygon -7500403 true true 73 210 86 251 62 249 48 208
Polygon -7500403 true true 25 114 16 195 9 204 23 213 25 200 39 123

cylinder
false
0
Circle -7500403 true true 0 0 300

dot
false
0
Circle -7500403 true true 90 90 120

face happy
false
0
Circle -7500403 true true 8 8 285
Circle -16777216 true false 60 75 60
Circle -16777216 true false 180 75 60
Polygon -16777216 true false 150 255 90 239 62 213 47 191 67 179 90 203 109 218 150 225 192 218 210 203 227 181 251 194 236 217 212 240

face neutral
false
0
Circle -7500403 true true 8 7 285
Circle -16777216 true false 60 75 60
Circle -16777216 true false 180 75 60
Rectangle -16777216 true false 60 195 240 225

face sad
false
0
Circle -7500403 true true 8 8 285
Circle -16777216 true false 60 75 60
Circle -16777216 true false 180 75 60
Polygon -16777216 true false 150 168 90 184 62 210 47 232 67 244 90 220 109 205 150 198 192 205 210 220 227 242 251 229 236 206 212 183

fish
false
0
Polygon -1 true false 44 131 21 87 15 86 0 120 15 150 0 180 13 214 20 212 45 166
Polygon -1 true false 135 195 119 235 95 218 76 210 46 204 60 165
Polygon -1 true false 75 45 83 77 71 103 86 114 166 78 135 60
Polygon -7500403 true true 30 136 151 77 226 81 280 119 292 146 292 160 287 170 270 195 195 210 151 212 30 166
Circle -16777216 true false 215 106 30

flag
false
0
Rectangle -7500403 true true 60 15 75 300
Polygon -7500403 true true 90 150 270 90 90 30
Line -7500403 true 75 135 90 135
Line -7500403 true 75 45 90 45

flower
false
0
Polygon -10899396 true false 135 120 165 165 180 210 180 240 150 300 165 300 195 240 195 195 165 135
Circle -7500403 true true 85 132 38
Circle -7500403 true true 130 147 38
Circle -7500403 true true 192 85 38
Circle -7500403 true true 85 40 38
Circle -7500403 true true 177 40 38
Circle -7500403 true true 177 132 38
Circle -7500403 true true 70 85 38
Circle -7500403 true true 130 25 38
Circle -7500403 true true 96 51 108
Circle -16777216 true false 113 68 74
Polygon -10899396 true false 189 233 219 188 249 173 279 188 234 218
Polygon -10899396 true false 180 255 150 210 105 210 75 240 135 240

house
false
0
Rectangle -7500403 true true 45 120 255 285
Rectangle -16777216 true false 120 210 180 285
Polygon -7500403 true true 15 120 150 15 285 120
Line -16777216 false 30 120 270 120

leaf
false
0
Polygon -7500403 true true 150 210 135 195 120 210 60 210 30 195 60 180 60 165 15 135 30 120 15 105 40 104 45 90 60 90 90 105 105 120 120 120 105 60 120 60 135 30 150 15 165 30 180 60 195 60 180 120 195 120 210 105 240 90 255 90 263 104 285 105 270 120 285 135 240 165 240 180 270 195 240 210 180 210 165 195
Polygon -7500403 true true 135 195 135 240 120 255 105 255 105 285 135 285 165 240 165 195

line
true
0
Line -7500403 true 150 0 150 300

line half
true
0
Line -7500403 true 150 0 150 150

pentagon
false
0
Polygon -7500403 true true 150 15 15 120 60 285 240 285 285 120

person
false
0
Circle -7500403 true true 110 5 80
Polygon -7500403 true true 105 90 120 195 90 285 105 300 135 300 150 225 165 300 195 300 210 285 180 195 195 90
Rectangle -7500403 true true 127 79 172 94
Polygon -7500403 true true 195 90 240 150 225 180 165 105
Polygon -7500403 true true 105 90 60 150 75 180 135 105

plant
false
0
Rectangle -7500403 true true 135 90 165 300
Polygon -7500403 true true 135 255 90 210 45 195 75 255 135 285
Polygon -7500403 true true 165 255 210 210 255 195 225 255 165 285
Polygon -7500403 true true 135 180 90 135 45 120 75 180 135 210
Polygon -7500403 true true 165 180 165 210 225 180 255 120 210 135
Polygon -7500403 true true 135 105 90 60 45 45 75 105 135 135
Polygon -7500403 true true 165 105 165 135 225 105 255 45 210 60
Polygon -7500403 true true 135 90 120 45 150 15 180 45 165 90

sheep
false
15
Circle -1 true true 203 65 88
Circle -1 true true 70 65 162
Circle -1 true true 150 105 120
Polygon -7500403 true false 218 120 240 165 255 165 278 120
Circle -7500403 true false 214 72 67
Rectangle -1 true true 164 223 179 298
Polygon -1 true true 45 285 30 285 30 240 15 195 45 210
Circle -1 true true 3 83 150
Rectangle -1 true true 65 221 80 296
Polygon -1 true true 195 285 210 285 210 240 240 210 195 210
Polygon -7500403 true false 276 85 285 105 302 99 294 83
Polygon -7500403 true false 219 85 210 105 193 99 201 83

square
false
0
Rectangle -7500403 true true 30 30 270 270

square 2
false
0
Rectangle -7500403 true true 30 30 270 270
Rectangle -16777216 true false 60 60 240 240

star
false
0
Polygon -7500403 true true 151 1 185 108 298 108 207 175 242 282 151 216 59 282 94 175 3 108 116 108

target
false
0
Circle -7500403 true true 0 0 300
Circle -16777216 true false 30 30 240
Circle -7500403 true true 60 60 180
Circle -16777216 true false 90 90 120
Circle -7500403 true true 120 120 60

tree
false
0
Circle -7500403 true true 118 3 94
Rectangle -6459832 true false 120 195 180 300
Circle -7500403 true true 65 21 108
Circle -7500403 true true 116 41 127
Circle -7500403 true true 45 90 120
Circle -7500403 true true 104 74 152

triangle
false
0
Polygon -7500403 true true 150 30 15 255 285 255

triangle 2
false
0
Polygon -7500403 true true 150 30 15 255 285 255
Polygon -16777216 true false 151 99 225 223 75 224

truck
false
0
Rectangle -7500403 true true 4 45 195 187
Polygon -7500403 true true 296 193 296 150 259 134 244 104 208 104 207 194
Rectangle -1 true false 195 60 195 105
Polygon -16777216 true false 238 112 252 141 219 141 218 112
Circle -16777216 true false 234 174 42
Rectangle -7500403 true true 181 185 214 194
Circle -16777216 true false 144 174 42
Circle -16777216 true false 24 174 42
Circle -7500403 false true 24 174 42
Circle -7500403 false true 144 174 42
Circle -7500403 false true 234 174 42

turtle
true
0
Polygon -10899396 true false 215 204 240 233 246 254 228 266 215 252 193 210
Polygon -10899396 true false 195 90 225 75 245 75 260 89 269 108 261 124 240 105 225 105 210 105
Polygon -10899396 true false 105 90 75 75 55 75 40 89 31 108 39 124 60 105 75 105 90 105
Polygon -10899396 true false 132 85 134 64 107 51 108 17 150 2 192 18 192 52 169 65 172 87
Polygon -10899396 true false 85 204 60 233 54 254 72 266 85 252 107 210
Polygon -7500403 true true 119 75 179 75 209 101 224 135 220 225 175 261 128 261 81 224 74 135 88 99

urchin
false
0
Circle -7500403 true true 90 90 120
Polygon -7500403 true true 105 180 15 150 105 135 45 60 120 120 105 15 165 120 210 15 195 165 285 105 120 135 300 210 135 165 150 300 180 180 60 255 105 150

wheel
false
0
Circle -7500403 true true 3 3 294
Circle -16777216 true false 30 30 240
Line -7500403 true 150 285 150 15
Line -7500403 true 15 150 285 150
Circle -7500403 true true 120 120 60
Line -7500403 true 216 40 79 269
Line -7500403 true 40 84 269 221
Line -7500403 true 40 216 269 79
Line -7500403 true 84 40 221 269

wolf
false
0
Polygon -16777216 true false 253 133 245 131 245 133
Polygon -7500403 true true 2 194 13 197 30 191 38 193 38 205 20 226 20 257 27 265 38 266 40 260 31 253 31 230 60 206 68 198 75 209 66 228 65 243 82 261 84 268 100 267 103 261 77 239 79 231 100 207 98 196 119 201 143 202 160 195 166 210 172 213 173 238 167 251 160 248 154 265 169 264 178 247 186 240 198 260 200 271 217 271 219 262 207 258 195 230 192 198 210 184 227 164 242 144 259 145 284 151 277 141 293 140 299 134 297 127 273 119 270 105
Polygon -7500403 true true -1 195 14 180 36 166 40 153 53 140 82 131 134 133 159 126 188 115 227 108 236 102 238 98 268 86 269 92 281 87 269 103 269 113

x
false
0
Polygon -7500403 true true 270 75 225 30 30 225 75 270
Polygon -7500403 true true 30 75 75 30 270 225 225 270

@#$#@#$#@
NetLogo 5.3.1
@#$#@#$#@
@#$#@#$#@
@#$#@#$#@
@#$#@#$#@
@#$#@#$#@
default
0.0
-0.2 0 0.0 1.0
0.0 1 1.0 0.0
0.2 0 0.0 1.0
link direction
true
0
Line -7500403 true 150 150 90 180
Line -7500403 true 150 150 210 180

@#$#@#$#@
0
@#$#@#$#@
