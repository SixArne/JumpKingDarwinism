<script setup lang="ts">
import { defineComponent } from 'vue';
import { useRouter } from 'vitepress'
//import { VPLink } from 'vitepress'
import { CustomCardProps } from './CustomCardProps'

let router = useRouter();

defineProps<{
    size?: 'small' | 'medium',
    project: CustomCardProps
}>();


</script>

<template>
  <article class="VPTeamprojectsItem" :class="[size ?? 'medium']">
    <div class="profile" @click="router.go(project.goto)">
      <figure class="avatar">
        <img class="avatar-img" :src="project.avatar" :alt="project.name">
      </figure>
      <div class="data">
        <h1 class="name">
          {{ project.name }}
        </h1>
        <p v-if="project.title || project.org" class="affiliation">
          <span v-if="project.title" class="title">
            {{ project.title }}
          </span>
          <span v-if="project.title && project.org" class="at">
            @
          </span>
          <VPLink v-if="project.org" class="org" :class="{ link: project.orgLink }" :href="project.orgLink" no-icon>
            {{ project.org }}
          </VPLink>
        </p>
        <p v-if="project.desc" class="desc">
          {{ project.desc }}
        </p>
      </div>
    </div>
  </article>
</template>

<style scoped>
.VPTeamprojectsItem {
  display: flex;
  flex-direction: column;
  gap: 2px;
  border-radius: 12px;
  width: 100%;
  height: 100%;
  overflow: hidden;
}

.VPTeamprojectsItem.small .profile {
  padding: 32px;
}

.profile:hover {
  background-color: rgba(0,150,0,0.1);
  cursor: pointer;
}

.VPTeamprojectsItem.small .data {
  padding-top: 20px;
}

.VPTeamprojectsItem.small .avatar {
  width: 64px;
  height: 64px;
}

.VPTeamprojectsItem.small .name {
  line-height: 24px;
  font-size: 16px;
}

.VPTeamprojectsItem.small .affiliation {
  padding-top: 4px;
  line-height: 20px;
  font-size: 14px;
}

.VPTeamprojectsItem.small .desc {
  padding-top: 12px;
  line-height: 20px;
  font-size: 14px;
}

.VPTeamprojectsItem.small .links {
  margin: 0 -16px -20px;
  padding: 10px 0 0;
}

.VPTeamprojectsItem.medium .profile {
  padding: 48px 32px;
}

.VPTeamprojectsItem.medium .data {
  padding-top: 24px;
  text-align: center;
}

.VPTeamprojectsItem.medium .avatar {
  width: 144px;
  height: 144px;
}

.VPTeamprojectsItem.medium .name {
  letter-spacing: 0.15px;
  line-height: 28px;
  font-size: 20px;
}

.VPTeamprojectsItem.medium .affiliation {
  padding-top: 4px;
  font-size: 16px;
}

.VPTeamprojectsItem.medium .desc {
  padding-top: 16px;
  max-width: 288px;
  font-size: 16px;
}

.VPTeamprojectsItem.medium .links {
  margin: 0 -16px -12px;
  padding: 16px 12px 0;
}

.profile {
  flex-grow: 1;
  background-color: var(--vp-c-bg-soft);
}

.data {
  text-align: center;
}

.avatar {
  position: relative;
  flex-shrink: 0;
  margin: 0 auto;
  border-radius: 20%;
  box-shadow: var(--vp-shadow-3);
}

.avatar-img {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  border-radius: 20%;
  object-fit: cover;
}

.name {
  margin:  0;
  font-weight: 600;
}

.affiliation {
  margin:  0;
  font-weight: 500;
  color: var(--vp-c-text-2);
}

.org.link {
  color: var(--vp-c-text-2);
  transition: color 0.25s;
}

.org.link:hover {
  color: var(--vp-c-brand);
}

.desc {
  margin: 0 auto;
}

.links {
  display: flex;
  justify-content: center;
  height: 56px;
}

.sp-link {
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
  padding: 16px;
  font-size: 14px;
  font-weight: 500;
  color: var(--vp-c-sponsor);
  background-color: var(--vp-c-bg-soft);
  transition: color 0.25s, background-color 0.25s;
}

.sp-link:hover,
.sp-link:focus {
  outline: none;
  color: var(--vp-c-text-dark-1);
  background-color: var(--vp-c-sponsor);
}

.sp-icon {
  margin-right: 8px;
  width: 16px;
  height: 16px;
  fill: currentColor;
}
</style>
