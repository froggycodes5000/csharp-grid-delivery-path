using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class DeliveryPathNode : MonoBehaviour
{
    public int X;
    public int Y;
    public DeliveryPathNode PrevPart;
    public DeliveryPathNode NextPart;
    public bool Active;
    public bool Flipped;
    public GameObject Source;

    public void Swap(int direction = 0)
    {
        DeliveryPathNode nextPart = NextPart;
        NextPart = PrevPart;
        PrevPart = nextPart;
        if (PrevPart != null && direction != 2)
        {
            PrevPart.Swap(1);
        }
        if (NextPart != null && direction != 1)
        {
            NextPart.Swap(2);
        }
    }

    public bool IsInPath(DeliveryPathNode part, int direction = 0)
    {
        if (part == PrevPart)
        {
            return true;
        }
        if (part = NextPart)
        { 
            return true;
        }
        if (PrevPart != null && direction != 2 && PrevPart.IsInPath(part, 1))
        {
            return true;
        }
        if (NextPart != null && direction != 1 && NextPart.IsInPath(part, 2))
        {
            return true;
        }
        return false;
    }

    public bool TryAttachPrimarySecondary(DeliveryPathNode primary, DeliveryPathNode secondary)
    {
        if (primary.NextPart == null)
        {
            if (secondary.PrevPart != null)
            {
                secondary.Swap();
            }
            if (secondary.PrevPart == null)
            {
                primary.NextPart = secondary;
                secondary.PrevPart = primary;
                secondary.SetActive(primary.Active);
                secondary.SetFlipped(primary.Flipped);
                return true;

            }

        }
        else if (primary.Source == null)
        {
            if (secondary.NextPart != null)
            {
                secondary.Swap();
            }
            if (secondary.NextPart == null)
            {
                primary.PrevPart = secondary;
                secondary.PrevPart = primary;
                secondary.SetActive(primary.Active);
                secondary.SetFlipped(primary.Flipped);
                return true;
                    
            }
        }
        return false;
    }

    public bool TryAttach(DeliveryPathNode part)
    {
        if (PrevPart != null && NextPart != null)
        {
            return false;
        }
        if (part.PrevPart != null && part.NextPart != null)
        {
            return false;
        }
        if (IsInPath(part))
        {
            return false;
        }
        if (Active && part.Active)
        {
            return false;
        }
        if (Active || (part.PrevPart == null && part.Source == null))
        {
            bool attached = TryAttachPrimarySecondary(this, part);
            if (attached)
            {
                return true;
            }
        }
        else
        {
            bool attached = TryAttachPrimarySecondary(part, this);
            if (attached)
            { 
                return true;
            }
        }
        return false;
    }
    public void detach()
    {
         if (PrevPart != null)
        {
            PrevPart.NextPart = null;
            PrevPart = null;
        }
         if (NextPart != null)
        {
            NextPart.PrevPart = null;
            NextPart.SetActive(false);
            NextPart.SetFlipped(false);
            NextPart = null;
        }
    }
    public void SetActive(bool active, int direction = 0)
    {
        Active = active;
        if (PrevPart != null && direction != 2)
        {
            PrevPart.SetActive(active, 1);
        }
        if (NextPart != null && direction != 1)
        {
            NextPart.SetActive(active, 2);
        }
    }
    public bool TryAttachSource(GameObject source)
    {
        if (PrevPart != null && NextPart != null)
        {
            return false;
        }
        /*if (source.Part != null)
        {
            return false;
        }*/
        if (source == null && ! Active)
        {
            if (PrevPart != null)
            {
                Swap();
            }
            if (PrevPart == null)
            {
                Source = source;
                //source.part = this;
                SetActive(true);
                return true;
            }
        }
        return false;
    }
    public void DetachSource()
    {
        if (Source != null)
        {
            Source = null;
            SetActive(false);
            SetFlipped(false);
        }
    }
    public void SetFlipped(bool flipped, int direction = 0)
    {
        Flipped = flipped;
        if (PrevPart != null & direction != 2)
        {
            PrevPart.SetFlipped(flipped, 1);
        }
        if (NextPart != null && direction != 1)
        {
            NextPart.SetFlipped(flipped, 2);
        }
    }
    public DeliveryPathNode GetHead()
    {
        if (PrevPart != null)
        {
            return PrevPart.GetHead();
        }
        return this;
    }
    public string GetType()
    {
        string prevDer = "_";
        string nextDer = "_";
        if (Source != null)
        { 
            if (Source.Y < Y)
            {
                string prevDir = "U";
            }
            else if (Source.Y  > Y)
            {
                string prevDir = "D";
            }
            else if (Source.X < X)
            {
                string prevDir = "L";
            }
            else
            {
                string prevDir = "R";
            }
        }
        if (PrevPart != null)
        {
            if (PrevPart.Y < Y)
            {
                string prevDir = "U";
            }
            else if (PrevPart.Y > Y)
            {
                string prevDir = "D";
            }
            else if (PrevPart.X < X)
            {
                string prevDir = "L";
            }
            else
            {
                string prevDir = "R";
            }
        }
        if (NextPart != null)
        {
            if (NextPart.Y < Y)
            {
                string nextDir = "U";
            }
            else if (NextPart.Y > Y)
            {
                string nextDir = "D";
            }
            else if (NextPart.X < X)
            {
                string nextDir = "L";
            }
            else
            {
                string nextDir = "R";
            }
        }
        return $"{prevDir}{nextDir}";
    }
    
}
public class DeliverySource
{
    public int X;
    public int Y;
    public DeliveryPathNode Part , null = null;
}
