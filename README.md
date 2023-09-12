# VR PROJECT - Damage and match logic outlines (PUN2)
Note: DisplayInputData, InputData, PunchHeuristic - are placed on each hand of the player and are responsible for hit value calculation

* Players spawn at respective corners
* Once both players are spawned, the match starts 
* Match consists of 3 (3 minute) rounds with breaks (1 minute) in between. Players are moved to center of the ring during the round and to respective corners during breaks.
* A Player wins by dealing more damage during the match - *Decision Win*, dealing 3 knockdowns (knockdown is when player runs out of health) to opponent in a single round - *Technical Knockout*, opponent disconnects/leaves the match - *Win by Disqualification*
* Once match is over, players are moved to center and are shown the results

# PunchHeuristic.cs
Determines punch/slap and specific punch type based on angle of the "knuckles".

"Knuckles" are denoted by an object on the forward axis of the player's hand.

# DisplayInputData.cs
Calculates force of the punch

Force = controller acceleration * fistMass (dummy value)

# DAMAGE CALCULATION
```
  private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead"))
        {
              velocity = hands.CalculateHitForce(isRight);
              isPunch = heuristic.ProcessCollision();
              if (isPunch)
              {
                  float scaledVel = Mathf.InverseLerp(0, 5000, velocity); 
                  opponent.TakeDamage(scaledVel);
              }
        }
    }
```

# PlayerHealthManager.cs
Handles health, regeneration, taking damage, "death"/knockdown.

# TIMER, PLAYER MOVEMENTS
When 2 players are ready, timer starts running. 3 minute rounds, 1 minute breaks. During rounds players are moved to the center and to respective corners during breaks. "Center" and "break" sectors are 2x2 meter squares.


I'm using "OVR Manager" of Oculus Integration package, to allow the player's boundary to be aligned with 2x2 virtual playspace, regardless of player's spawnpoint position.

When moving the player during rounds/breaks, relative repositioning is used instead of absolute, ie.: currentPosition.X += 2, instead of currentPositionX = 2
Example from code:
```
  if (networkedTimer.timer1_active == true)
  {
      if (OnlyOnce1 == false)
      {
          if (blue)
          {
              this.transform.localPosition = new Vector3(this.transform.position.x - 3, this.transform.position.y,
                  this.transform.position.z - 3);
          }
          if (!blue)
          {
              this.transform.localPosition = new Vector3(this.transform.position.x + 3, this.transform.position.y,
                  this.transform.position.z + 3);
          }


          OnlyOnce1 = true;
      }
  }
```
In example above, client checks if timer for Round 1 has started, it then moves the player from either blue or red corner to center.



