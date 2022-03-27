import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { Activity } from "../models/Activity";
import { v4 as uuid } from 'uuid';

export default class ActivityStore {
    activityRegistry = new Map<string, Activity>();
    selectedActivity: Activity | undefined = undefined;
    editMode = false;
    loading = false;
    loadingInitial = true;

    constructor() {
        makeAutoObservable(this)
    }

    get activitiesByDate() {
        return Array.from(this.activityRegistry.values()).sort((a, b) =>
            Date.parse(a.date) - Date.parse(b.date));
    }

    loadActivities = async () => {
        try {
            const activities = await agent.Activites.list();

            activities.forEach(a => {
                a.date = a.date.split('T')[0];
                this.activityRegistry.set(a.id, a);
            })
            this.setLoadingInitial(false);
        } catch (error) {
            console.error(error);
            this.setLoadingInitial(false);
        }

    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    selectActivity = (id: string) => {
        this.selectedActivity = this.activityRegistry.get(id);
    }

    cancelSelectedActivity = () => {
        this.selectedActivity = undefined;
    }

    openForm = (id?: string) => {
        id ? this.selectActivity(id) : this.cancelSelectedActivity();
        this.editMode = true;
    }

    closeForm = () => {
        this.editMode = false;
    }

    createActivity = async (activity: Activity) => {
        this.loading = true;
        activity.id = uuid();

        try {
            await agent.Activites.create(activity);
            runInAction(() => {
                this.activityRegistry.set(activity.id, activity);
                this.selectedActivity = activity;
                this.editMode = false;
                this.loading = false;
            })
        } catch (error) {
            console.error(error);
            runInAction(() => { this.loading = false })
        }
    }

    updateActivity = async (activity: Activity) => {
        this.loading = true;

        try {
            await agent.Activites.update(activity);
            runInAction(() => {
                //this.activities = [...this.activities.filter(a => a.id!== activity.id),activity];
                this.activityRegistry.set(activity.id, activity);
                this.selectedActivity = activity;
                this.editMode = false;
                this.loading = false;
            })
        } catch (error) {
            console.error(error);
            runInAction(() => { this.loading = false })
        }
    }

    deleteActivity = async (id: string) => {
        this.loading = true;

        try {
            await agent.Activites.delete(id);
            runInAction(() => {
                this.activityRegistry.delete(id);
                if (this.selectedActivity?.id === id) this.cancelSelectedActivity();
                this.loading = false;
            })
        } catch (error) {
            console.error(error);
            runInAction(() => { this.loading = false })
        }
    }
}